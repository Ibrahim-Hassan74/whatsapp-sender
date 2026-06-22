using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WhatsAppServices.API.Data;
using WhatsAppServices.API.DTO;
using WhatsAppServices.API.Entities;
using WhatsAppServices.API.Enums;
using WhatsAppServices.API.Hubs;
using WhatsAppServices.API.ServicesContract;

namespace WhatsAppServices.API.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IHubContext<WhatsAppHub> _hub;
        private readonly IMemoryCache _cache;

        public WhatsAppService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            IHubContext<WhatsAppHub> hub,
            IMemoryCache cache)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _hub = hub;
            _cache = cache;
        }

        public async Task<Result<SendMessageResponse>> SendMessageAsync(SendMessageRequest request)
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                Number = request.Number,
                Content = request.Message,
                CreatedAt = DateTime.UtcNow,
                IsSent = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            try
            {
                var client = CreateNodeClient();

                var response = await client.PostAsJsonAsync("/send", new
                {
                    number = request.Number,
                    message = request.Message
                });

                var isSuccess = response.IsSuccessStatusCode;

                message.IsSent = isSuccess;
                message.SentAt = isSuccess ? DateTime.UtcNow : null;
                message.Error = isSuccess ? null : "Failed from Node";

                await _context.SaveChangesAsync();

                await LogAttempt(message.Id, isSuccess, message.Error);

                if (!isSuccess)
                    return Result<SendMessageResponse>.Failure("Send failed");

                return Result<SendMessageResponse>.Success(new SendMessageResponse
                {
                    Number = request.Number,
                    Message = request.Message,
                    SentAt = message.SentAt!.Value
                });
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                await _context.SaveChangesAsync();

                await LogAttempt(message.Id, false, ex.Message);

                return Result<SendMessageResponse>.Failure(ex.Message);
            }
        }

        public async Task<Result<ConnectionStatusResponse>> CheckStatusAsync()
        {
            try
            {
                var client = CreateNodeClient();

                var response = await client.GetAsync("/status");

                if (!response.IsSuccessStatusCode)
                    return Result<ConnectionStatusResponse>.Failure("Node not reachable");

                var result = await response.Content.ReadFromJsonAsync<NodeStatusResponse>();

                if (result is null)
                    return Result<ConnectionStatusResponse>.Failure("Invalid response from Node");

                var session = await _context.WhatsAppSessions.FirstOrDefaultAsync();

                if (session is null)
                {
                    session = new WhatsAppSession
                    {
                        Id = Guid.NewGuid()
                    };
                    _context.WhatsAppSessions.Add(session);
                }

                session.IsConnected = result.Connected;
                session.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Result<ConnectionStatusResponse>.Success(new ConnectionStatusResponse
                {
                    IsConnected = result.Connected,
                    LastUpdated = session.LastUpdated
                });
            }
            catch (Exception ex)
            {
                var session = await _context.WhatsAppSessions.FirstOrDefaultAsync();

                if (session is not null)
                {
                    return Result<ConnectionStatusResponse>.Success(new ConnectionStatusResponse
                    {
                        IsConnected = session.IsConnected,
                        LastUpdated = session.LastUpdated
                    });
                }

                return Result<ConnectionStatusResponse>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> LogoutAsync()
        {
            try
            {
                var client = CreateNodeClient();

                var response = await client.PostAsync("/logout", null);

                return Result<bool>.Success(response.IsSuccessStatusCode);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> UpdateQrAsync(string qr)
        {
            _cache.Set("whatsapp:qr", qr, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(360)
            });

            var session = await _context.WhatsAppSessions.FirstOrDefaultAsync();

            if (session is null)
            {
                session = new WhatsAppSession
                {
                    Id = Guid.NewGuid()
                };
                _context.WhatsAppSessions.Add(session);
            }

            session.LastQrCode = qr;
            session.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ReceiveQR", qr);

            return Result<bool>.Success(true);
        }

        public async Task<Result<QrCodeResponse>> GetLastQr()
        {
            if (_cache.TryGetValue("whatsapp:qr", out string qr) && !string.IsNullOrEmpty(qr))
            {
                return Result<QrCodeResponse>.Success(new QrCodeResponse
                {
                    QrCode = qr,
                    IsAvailable = true
                });
            }

            var session = await _context.WhatsAppSessions.FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(session?.LastQrCode))
            {
                _cache.Set("whatsapp:qr", session.LastQrCode, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                });

                return Result<QrCodeResponse>.Success(new QrCodeResponse
                {
                    QrCode = session.LastQrCode,
                    IsAvailable = true
                });
            }

            return Result<QrCodeResponse>.Failure("QR not available");
        }

        public async Task<Result<bool>> UpdateConnectionStatusAsync(bool isConnected)
        {
            var session = await _context.WhatsAppSessions.FirstOrDefaultAsync();

            if (session is null)
            {
                session = new WhatsAppSession
                {
                    Id = Guid.NewGuid()
                };
                _context.WhatsAppSessions.Add(session);
            }

            session.IsConnected = isConnected;
            session.LastUpdated = DateTime.UtcNow;

            if (isConnected)
            {
                _cache.Remove("whatsapp:qr");

                session.LastQrCode = null;
            }

            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ConnectionStatusChanged", isConnected);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> LogMessageAsync(string number, string message, bool isSent)
        {
            var entity = new Message
            {
                Id = Guid.NewGuid(),
                Number = number,
                Content = message,
                IsSent = isSent,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(entity);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<PagedResponse<MessageDto>>> GetMessagesAsync(PaginationRequest request)
        {
            var query = _context.Messages.AsQueryable();

            var totalCount = await query.CountAsync();

            var messages = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new MessageDto
                {
                    Number = x.Number,
                    Message = x.Content,
                    IsSent = x.IsSent,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var response = new PagedResponse<MessageDto>
            {
                Data = messages,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return Result<PagedResponse<MessageDto>>.Success(response);
        }

        private HttpClient CreateNodeClient()
        {
            var client = _httpClientFactory.CreateClient();

            client.BaseAddress = new Uri(_config["Node:BaseUrl"]);

            client.DefaultRequestHeaders.Add("x-node-token", _config["Node:Token"]);
            client.DefaultRequestHeaders.Add("x-server-name", _config["Node:ServerName"]);

            return client;
        }

        private async Task LogAttempt(Guid messageId, bool success, string? error)
        {
            var attempt = new MessageAttempt
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                AttemptedAt = DateTime.UtcNow,
                IsSuccess = success,
                Error = error
            };

            _context.MessageAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }
    }
}
