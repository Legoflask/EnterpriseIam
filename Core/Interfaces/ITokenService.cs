using EnterpriseIam.Core.Entities;

namespace EnterpriseIam.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
}