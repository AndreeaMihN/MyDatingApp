
namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    public AccountController(UserManager<AppUser> userManger, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
    {
        _signInManager = signInManager;
        _userManger = userManger;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        var user = _mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();

        var result = await _userManger.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var roleResult = await _userManger.AddToRoleAsync(user, "Member");
        if (!roleResult.Succeeded) return BadRequest(result.Errors);

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManger.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
        if (user == null) return Unauthorized("Invalid username");

        var results = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!results.Succeeded) return Unauthorized();

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManger.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
