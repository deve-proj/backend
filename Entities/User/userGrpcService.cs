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
            Id = user.UserId,
            Name = user.Name,
            Legend = user.Legend,
            Avatar = user.Avatar,
            Login = user.Login,
            ReputationScore = user.ReputationScore
        };
    }

    public override async Task<UserListResponse> GetUsers(UserListRequest request, ServerCallContext context)
    {

        string[] userIds = request.Ids.ToArray();

        var result = await _userService.GetUsersInfo(userIds);

        UserListResponse response = new UserListResponse();
        
        foreach(var user in result)
        {
            response.Users.Add(new UserResponse
            {
                Id = user.UserId,
                Name = user.Name,
                Legend = user.Legend,
                Avatar = user.Avatar,
                Login = user.Login,
                ReputationScore = user.ReputationScore
            });
        }

        return response;
    }
}