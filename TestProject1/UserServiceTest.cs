#nullable disable
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

        /// <summary>
        /// Инициализирует новый экземпляр тестового класса.
        /// Создаёт экземпляры <see cref="HashService"/> и <see cref="UserService"/>.
        /// </summary>
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
        /// <exception cref="InvalidOperationException">Ожидается при дублировании имени.</exception>
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
        /// <exception cref="InvalidOperationException">Ожидается при попытке входа в заблокированный аккаунт.</exception>
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
        /// <exception cref="InvalidOperationException">Ожидается при неверном старом пароле.</exception>
        [Fact]
        public void ChangePassword_InvalidOldPassword_ThrowsInvalidOperationException()
        {
            _userService.RegisterUser("changeme", "old");
            Assert.Throws<InvalidOperationException>(() => _userService.ChangePassword("changeme", "wrong", "new"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что регистрация с пустым именем пользователя вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом имени.</exception>
        [Fact]
        public void RegisterUser_EmptyUsername_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.RegisterUser("", "pass"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что регистрация с пустым паролем вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом пароле.</exception>
        [Fact]
        public void RegisterUser_EmptyPassword_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.RegisterUser("user", ""));
        }

        /// <summary>
        /// Проверяет, что LoadUsers заменяет внутренний список пользователей.
        /// </summary>
        [Fact]
        public void LoadUsers_ReplacesList()
        {
            var newUsers = new System.Collections.Generic.List<HashSystem.Models.UserCredential>
            {
                new HashSystem.Models.UserCredential("loaded", "hash", "salt", "SHA256")
            };
            _userService.LoadUsers(newUsers);
            var user = _userService.GetUser("loaded");
            Assert.NotNull(user);
            Assert.Equal("loaded", user.Username);
        }

        /// <summary>
        /// Проверяет, что CountLockedUsers возвращает правильное количество заблокированных.
        /// </summary>
        [Fact]
        public void CountLockedUsers_ReturnsCorrectCount()
        {
            _userService.RegisterUser("user1", "pass1");
            _userService.RegisterUser("user2", "pass2");
            for (int i = 0; i < 5; i++)
            {
                try { _userService.VerifyPassword("user1", "wrong"); }
                catch (InvalidOperationException) { }
            }
            int locked = _userService.CountLockedUsers();
            Assert.Equal(1, locked);
        }

        /// <summary>
        /// Проверяет, что смена пароля на тот же самый пароль работает (или не работает – по логике должно работать).
        /// </summary>
        [Fact]
        public void ChangePassword_SamePassword_Works()
        {
            _userService.RegisterUser("same", "pass");
            _userService.ChangePassword("same", "pass", "pass");
            Assert.True(_userService.VerifyPassword("same", "pass"));
        }

        /// <summary>
        /// Проверяет, что после блокировки даже верный пароль вызывает исключение.
        /// </summary>
        /// <exception cref="InvalidOperationException">Ожидается при попытке входа в заблокированный аккаунт.</exception>
        [Fact]
        public void VerifyPassword_LockedAccount_ThrowsEvenWithCorrectPassword()
        {
            _userService.RegisterUser("locked", "correct");
            for (int i = 0; i < 5; i++)
            {
                try { _userService.VerifyPassword("locked", "wrong"); }
                catch (InvalidOperationException) { }
            }
            Assert.Throws<InvalidOperationException>(() => _userService.VerifyPassword("locked", "correct"));
        }

        /// <summary>
        /// Проверяет, что ChangePassword с пустым новым паролем вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом новом пароле.</exception>
        [Fact]
        public void ChangePassword_EmptyNewPassword_ThrowsArgumentNullException()
        {
            _userService.RegisterUser("test", "old");
            Assert.Throws<ArgumentNullException>(() => _userService.ChangePassword("test", "old", ""));
        }

        /// <summary>
        /// Проверяет, что GetUser с пустым именем вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом имени.</exception>
        [Fact]
        public void GetUser_EmptyUsername_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.GetUser(""));
        }

        /// <summary>
        /// Проверяет, что LoadUsers с null вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void LoadUsers_NullList_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _userService.LoadUsers(null));
        }
        
        /// <summary>
        /// Проверяет, что GetUser возвращает null для несуществующего пользователя.
        /// Покрывает ветку, когда пользователь не найден (Find возвращает null).
        /// </summary>
        [Fact]
        public void GetUser_NonExistentUser_ReturnsNull()
        {
            var user = _userService.GetUser("nonexistent_user_12345");
            Assert.Null(user);
        }


       

       

        

     
    }
}