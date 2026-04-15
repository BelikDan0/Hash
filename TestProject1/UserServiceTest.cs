using System;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
    /// <summary>
    /// Модульные тесты для сервиса управления пользователями <see cref="UserService"/>.
    /// </summary>
    public class UserServiceTests
    {
        private readonly HashService _hashService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _hashService = new HashService();
            _userService = new UserService(_hashService);
        }

        /// <summary>
        /// TC-08: Проверяет, что регистрация нового пользователя добавляет его в список.
        /// </summary>
        [Fact]
        public void RegisterUser_NewUser_UserAdded()
        {
            _userService.RegisterUser("newuser", "password");
            var user = _userService.GetUser("newuser");
            Assert.NotNull(user);
            Assert.Equal("newuser", user.Username);
        }

        /// <summary>
        /// TC-09: Проверяет, что попытка зарегистрировать пользователя с уже существующим именем вызывает InvalidOperationException.
        /// </summary>
        [Fact]
        public void RegisterUser_Duplicate_ThrowsInvalidOperationException()
        {
            _userService.RegisterUser("duplicate", "pass");
            Assert.Throws<InvalidOperationException>(() => _userService.RegisterUser("duplicate", "pass2"));
        }

        /// <summary>
        /// TC-10: Проверяет, что VerifyPassword возвращает true для правильной пары (логин, пароль).
        /// </summary>
        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            _userService.RegisterUser("testuser", "correct");
            bool result = _userService.VerifyPassword("testuser", "correct");
            Assert.True(result);
        }

        /// <summary>
        /// TC-11: Проверяет, что VerifyPassword возвращает false для неверного пароля.
        /// </summary>
        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            _userService.RegisterUser("testuser", "correct");
            bool result = _userService.VerifyPassword("testuser", "wrong");
            Assert.False(result);
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что после 5 неудачных попыток входа аккаунт блокируется,
        /// а при попытке входа (даже с верным паролем) выбрасывается InvalidOperationException.
        /// </summary>
        [Fact]
        public void VerifyPassword_FiveFailedAttempts_LocksAccount()
        {
            _userService.RegisterUser("lockme", "pass");

            for (int i = 0; i < 4; i++)
            {
                _userService.VerifyPassword("lockme", "wrong");
            }

            // Пятая неудачная попытка должна заблокировать аккаунт и выбросить исключение
            Assert.Throws<InvalidOperationException>(() => _userService.VerifyPassword("lockme", "wrong"));
            var user = _userService.GetUser("lockme");
            Assert.True(user.IsLocked);
        }

        /// <summary>
        /// Дополнительный тест: проверяет успешную смену пароля при правильно введённом старом пароле.
        /// </summary>
        [Fact]
        public void ChangePassword_ValidOldPassword_PasswordChanged()
        {
            _userService.RegisterUser("changeme", "old");
            _userService.ChangePassword("changeme", "old", "new");
            Assert.True(_userService.VerifyPassword("changeme", "new"));
            Assert.False(_userService.VerifyPassword("changeme", "old"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что при смене пароля с неверным старым паролем выбрасывается InvalidOperationException.
        /// </summary>
        [Fact]
        public void ChangePassword_InvalidOldPassword_ThrowsInvalidOperationException()
        {
            _userService.RegisterUser("changeme", "old");
            Assert.Throws<InvalidOperationException>(() => _userService.ChangePassword("changeme", "wrong", "new"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что регистрация с пустым именем пользователя вызывает ArgumentNullException.
        /// </summary>
        [Fact]
        public void RegisterUser_EmptyUsername_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.RegisterUser("", "pass"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что регистрация с пустым паролем вызывает ArgumentNullException.
        /// </summary>
        [Fact]
        public void RegisterUser_EmptyPassword_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.RegisterUser("user", ""));
        }
    }
}