namespace AlphaLogistics.API.Services
{
    using System.Security.Claims;

    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User.FindFirstValue(ClaimTypes.NameIdentifier);

                return long.TryParse(userId, out var id) ? id : 0;
            }
        }
    }

}
