using Grpc.Core;

public class UserGrpcServcice : UserGrpc.UserGrpcBase
{
    private readonly IUserService _userService;

    public UserGrpcServcice(IUserService userService)
    {
        _userService = userService;
    }

    public override async Task<UserResponse> GetUser(UserRequest request, ServerCallContext context)
    {

        var user = await _userService.GetUserInfo(request.Id);

        return new UserResponse
        {
            Id = request.Id,
            Name = user.Name,
            Legend = user.Legend,
            Avatar = user.Avatar,
            Login = user.Login,
            ReputationScore = user.ReputationScore
        };
    }
}