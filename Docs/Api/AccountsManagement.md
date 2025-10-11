### Accounts Management Service


## DTO:

# Register Request DTO 

DTO запроса на регистрацию, в JSON выглядит как:

```

{
  "login": "string",
  "password": "string"
}

```

---

# Login Request DTO

DTO запроса на регистрацию, в JSON выглядит так:

```

{
  "login": "string",
  "password": "string"
}

```

P.S Да, он сейчас идентичен Register Request DTO, но это сделано что-бы не было проблем если вдруг придется отправлять что то еще.

---

# Account Recovery Request DTO

```

{
  "login": "string",
  "recoveryCode": "string",
  "newPassword": "string"
}


```


## Актуальные Эндпоинты:

# POST /register

Принимает Register Request DTO (см. раздел с DTO)

может отдать 2 ответа:

 - 400 - Account registration failed. Совпадение логина с уже зарегистрированным аккаунтом.

 - 200 - Регистрация прошла успешно, возвращает JSON 

  ```
    {
      "recoveryCode": "string"
      "refreshToken": "string"
    }
  ```

  recoveryCode - Код для восстановления аккаунта, 20 символов


# POST /login 

Принимает Login Request DTO (см. раздел с DTO)


Может отдать 2 ответа:

 - 400 - Login Failed. Неправильный логин/пароль.
 - 200 - Логин прошел успешно, возвращает UserData (На момент заполнения документации - пустоту).


# POST /recovery

Принимает Account Recovery Request DTO (см. раздел с DTO)

Может отдать 2 ответа:

 - 400 - Ошибка восстановления аккаунта ("Password reset failed"). Неправильный код восстановления
 - 200 - Успешное изменение пароля, возвращает новый код восстановления аккаунта (старый больше невалиден), JSON ответ:

   ```
    {
    "newRecovery": "string"
    }
  ```