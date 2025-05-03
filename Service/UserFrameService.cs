using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Service
{
    public interface IUserFrameService
    {
        Task<string> GetFrameImageByUserIdAsync(int userId, int? currentFrameId);
    }
    public class UserFrameService : IUserFrameService
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public UserFrameService(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        public async Task<string> GetFrameImageByUserIdAsync(int userId, int? currentFrameId)
        {
            
            currentFrameId ??= -1;

            
            var userFrames = await _context.UserFrames
                .Include(uf => uf.Frame)
                .Where(uf => uf.UserId == userId)
                .ToListAsync();

            
            var matchedFrame = userFrames.FirstOrDefault(uf => uf.FrameId == currentFrameId);

            
            if (matchedFrame?.Frame != null)
            {
                return matchedFrame.Frame.FrameImage;
            }

            return null;
        }
    }
}
