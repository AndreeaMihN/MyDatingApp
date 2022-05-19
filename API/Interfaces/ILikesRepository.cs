
namespace API.Interfaces;
public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int sourceUserid, int likedUserId);
    Task<AppUser> GetUserWithLikes(int userId);
    Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
}