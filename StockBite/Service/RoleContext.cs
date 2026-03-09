using Microsoft.AspNetCore.Http;
using StockBite.Models;
using StockBite.Services;

namespace StockBite.Services
{
    public class RoleContext : IRoleContext
    {
        private const string RoleKey = "CurrentUserRole";
        private const string VendorKey = "CurrentVendorId";
        private const string ConsumerKey = "CurrentConsumerId";
        private const string AuthKey = "IsAuthenticated";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserRole CurrentRole
        {
            get
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                {
                    return UserRole.Public;
                }

                var roleString = session.GetString(RoleKey);
                if (Enum.TryParse(roleString, true, out UserRole role))
                {
                    return role;
                }

                return UserRole.Public;
            }
        }

        public void SetRole(UserRole role)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.SetString(RoleKey, role.ToString());
        }

        public int? CurrentVendorId
        {
            get
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                {
                    return null;
                }

                var vendorIdString = session.GetString(VendorKey);
                if (int.TryParse(vendorIdString, out var vendorId))
                {
                    return vendorId;
                }

                return null;
            }
        }

        public void SetVendorId(int? vendorId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }

            if (vendorId.HasValue)
            {
                session.SetString(VendorKey, vendorId.Value.ToString());
            }
            else
            {
                session.Remove(VendorKey);
            }
        }

        public int? CurrentConsumerId
        {
            get
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                {
                    return null;
                }

                var consumerIdString = session.GetString(ConsumerKey);
                if (int.TryParse(consumerIdString, out var consumerId))
                {
                    return consumerId;
                }

                return null;
            }
        }

        public void SetConsumerId(int? consumerId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }

            if (consumerId.HasValue)
            {
                session.SetString(ConsumerKey, consumerId.Value.ToString());
            }
            else
            {
                session.Remove(ConsumerKey);
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                {
                    return false;
                }

                return session.GetString(AuthKey) == "true";
            }
        }

        public void SetAuthenticated(bool isAuthenticated)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }

            session.SetString(AuthKey, isAuthenticated ? "true" : "false");
        }
    }
}
