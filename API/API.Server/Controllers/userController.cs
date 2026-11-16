using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DAL;
using Model;

namespace API.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class userController : ControllerBase
    {
        private readonly userDal _userDal;

        public userController(userDal userDal)
        {
            _userDal = userDal;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userDal.GetAllUsersAsync();
                foreach (var u in users)
                {
                    u.Password = string.Empty;
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "שגיאה בשליפת המשתמשים", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest(new { message = "נתוני הרשמה אינם תקינים" });
            }

            try
            {
                bool exists = await _userDal.IsEmailExistsAsync(user.Email);
                if (exists)
                {
                    return BadRequest(new { message = "כתובת האימייל כבר קיימת במערכת" });
                }

                bool success = await _userDal.AddUserAsync(user);
                if (success)
                {
                    return Ok(new { message = "המשתמש נרשם בהצלחה!" });
                }
                return BadRequest(new { message = "הרשמת המשתמש נכשלה" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "שגיאה בהרשמת המשתמש", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "פרטי התחברות חסרים" });
            }

            try
            {
                var user = await _userDal.LoginAsync(request.Email, request.Password);
                if (user == null)
                {
                    return Unauthorized(new { message = "שם המשתמש או הסיסמה שגויים" });
                }

                // ריקון הסיסמה/גיבוב לפני החזרת המשתמש מה-API
                user.Password = string.Empty;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "שגיאה בתהליך ההתחברות", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                bool success = await _userDal.DeleteUserAsync(id);
                if (success)
                {
                    return Ok(new { message = "המשתמש הועבר למצב לא-פעיל בהצלחה!" });
                }
                return NotFound(new { message = "המשתמש לא נמצא" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "שגיאה בביטול המשתמש", error = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}