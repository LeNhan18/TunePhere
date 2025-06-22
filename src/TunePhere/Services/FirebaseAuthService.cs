using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using TunePhere.Models;

namespace TunePhere.Services
{
    public class FirebaseAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public FirebaseAuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AppUser> AuthenticateFirebaseUserAsync(string idToken)
        {
            try
            {
                // Xác thực token với Firebase
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;
                string email = decodedToken.Claims["email"].ToString();
                string name = decodedToken.Claims["name"].ToString();

                // Tìm user trong database
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // Tạo user mới nếu chưa tồn tại
                    user = new AppUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Không thể tạo tài khoản mới");
                    }
                }

                // Đăng nhập user
                await _signInManager.SignInAsync(user, isPersistent: true);

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi xác thực Firebase: " + ex.Message);
            }
        }
    }
} 