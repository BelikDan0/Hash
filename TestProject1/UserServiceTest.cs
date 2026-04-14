using System;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
    public class UserServiceTests
    {
        private readonly HashService _hashService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _hashService = new HashService();
            _userService = new UserService(_hashService);
        }

        // TC-08: Регистрация нового пользователя
        [Fact]
        public void RegisterUser_NewUser_UserAdded()
        {
            _userService.RegisterUser("newuser", "password");
            var user = _userService.GetUser("newuser");
            Assert.NotNull(user);
            Assert.Equal("newuser", user.Username);
        }

        // TC-09: Регистрация дубликата -> InvalidOperationException
        [Fact]
        public void RegisterUser_Duplicate_ThrowsInvalidOperationException()
        {
            _userService.RegisterUser("duplicate", "pass");
            Assert.Throws<InvalidOperationException>(() => _userService.RegisterUser("duplicate", "pass2"));
        }

        // TC-10: Верный пароль -> true
        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            _userService.RegisterUser("testuser", "correct");
            bool result = _userService.VerifyPassword("testuser", "correct");
            Assert.True(result);
        }

        // TC-11: Неверный пароль -> false
        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            _userService.RegisterUser("testuser", "correct");
            bool result = _userService.VerifyPassword("testuser", "wrong");
            Assert.False(result);
        }

        // Доп. тест: блокировка после 5 неудачных попыток
        [Fact]
        public void VerifyPassword_FiveFailedAttempts_LocksAccount()
        {
            _userService.RegisterUser("lockme", "pass");

            // 4 неудачные попытки (ещё не блокируют)
            for (int i = 0; i < 4; i++)
            {
                _userService.VerifyPassword("lockme", "wrong");
            }

            // Пятая неудачная попытка должна заблокировать аккаунт и выбросить исключение
            Assert.Throws<InvalidOperationException>(() => _userService.VerifyPassword("lockme", "wrong"));

            var user = _userService.GetUser("lockme");
            Assert.True(user.IsLocked);
        }

        // Доп. тест: смена пароля
        [Fact]
        public void ChangePassword_ValidOldPassword_PasswordChanged()
        {
            _userService.RegisterUser("changeme", "old");
            _userService.ChangePassword("changeme", "old", "new");
            Assert.True(_userService.VerifyPassword("changeme", "new"));
            Assert.False(_userService.VerifyPassword("changeme", "old"));
        }

        // Доп. тест: смена пароля с неверным старым паролем
        [Fact]
        public void ChangePassword_InvalidOldPassword_ThrowsInvalidOperationException()
        {
            _userService.RegisterUser("changeme", "old");
            Assert.Throws<InvalidOperationException>(() => _userService.ChangePassword("changeme", "wrong", "new"));
        }

        // Доп. тест: пустое имя пользователя
        [Fact]
        public void RegisterUser_EmptyUsername_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.RegisterUser("", "pass"));
        }

        // Доп. тест: пустой пароль
        [Fact]
        public void RegisterUser_EmptyPassword_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.RegisterUser("user", ""));
        }
    }
}