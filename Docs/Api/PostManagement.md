### Post Management Service

## DTO

### PostCreationRequestDTO
```
{
  "content": "string"
}
```

### LikePostRequestDTO
```
{
  "postId": "string"
}
```

### CommentPostRequestDTO
```
{
  "postId": "string",
  "content": "string"
}
```

---

## Эндпоинты

### POST /create
- **Auth**: Требуется JWT
- **Rate limit**: 20 запросов/мин
- **Body**: PostCreationRequestDTO
- **200**: Возвращает созданный объект поста
- **400**: `Post creation Failed`

Пример ответа 200:
```
{
  "id": "string",
  "authorId": "string",
  "content": "string",
  "createdAt": "2025-01-01T00:00:00Z",
  "likes": 0,
  "commentsCount": 0
}
```

---

### GET /getPosts/{count}
- **Auth**: Требуется JWT
- **Rate limit**: 20 запросов/мин
- **Params**:
  - `count`: количество постов к выдаче
- **200**: `{ "posts": Post[] }`
- **400**: `Post get Failed`

---

### POST /likePost
- **Auth**: Требуется JWT
- **Rate limit**: 20 запросов/мин
- **Body**: LikePostRequestDTO
- **200**: OK
- **400**: `Like Failed`

---

### POST /commentPost
- **Auth**: Требуется JWT
- **Rate limit**: 20 запросов/мин
- **Body**: CommentPostRequestDTO
- **200**: OK
- **400**: `Comment Failed`

---

### GET /getComments/{postid}/chunk/{chunkid}
- **Auth**: Требуется JWT
- **Rate limit**: 20 запросов/мин
- **Params**:
  - `postid`: идентификатор поста
  - `chunkid`: номер чанка (целое число)
- **200**: Возвращает массив комментариев выбранного чанка
- **400**: `Post or chunk not found` или `Chunk ID is invalid`

---

## Rate Limits ⚡
- **Глобальный лимит**: 20 запросов в минуту
- **При превышении**: бан по IP на 5 минут


