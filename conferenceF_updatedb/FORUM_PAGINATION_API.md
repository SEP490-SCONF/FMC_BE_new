# Forum Questions Pagination API

## Endpoint

```
GET /api/ForumQuestions/forum/{forumId}/paginated
```

## Description

Truy xuất danh sách các câu hỏi trong forum với phân trang và tìm kiếm. Mỗi câu hỏi sẽ hiển thị 2 câu trả lời mới nhất đi kèm.

## Parameters

### Path Parameters
- `forumId` (int, required): ID của forum cần lấy câu hỏi

### Query Parameters
- `page` (int, optional, default: 1): Trang hiện tại (bắt đầu từ 1)
- `pageSize` (int, optional, default: 10): Số lượng câu hỏi trên mỗi trang
- `search` (string, optional): Từ khóa tìm kiếm trong Title, Description, hoặc Question

## Request Example

```http
GET /api/ForumQuestions/forum/1/paginated?page=1&pageSize=5&search=technology
```

## Response

### Success Response (200 OK)

```json
{
  "questions": [
    {
      "fqId": 1,
      "askBy": 123,
      "forumId": 1,
      "title": "How to implement technology X?",
      "description": "I need guidance on implementing...",
      "question": "What are the best practices for...?",
      "createdAt": "2024-01-15T10:30:00Z",
      "askerName": "John Doe",
      "askerEmail": "john@example.com",
      "totalAnswers": 5,
      "totalLikes": 8,
      "recentAnswers": [
        {
          "answerId": 101,
          "answerBy": 456,
          "answer": "You can start by following these steps...",
          "createdAt": "2024-01-15T14:20:00Z",
          "answererName": "Jane Smith",
          "answererEmail": "jane@example.com",
          "parentAnswerId": null
        },
        {
          "answerId": 102,
          "answerBy": 789,
          "answer": "I agree with the previous answer, but also...",
          "createdAt": "2024-01-15T15:10:00Z",
          "answererName": "Bob Wilson",
          "answererEmail": "bob@example.com",
          "parentAnswerId": 101
        }
      ]
    }
  ],
  "totalCount": 25,
  "currentPage": 1,
  "pageSize": 5,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "searchTerm": "technology"
}
```

### Error Responses

#### Forum Not Found (404 Not Found)
```json
{
  "message": "Forum with ID 999 not found."
}
```

#### Internal Server Error (500 Internal Server Error)
```json
{
  "message": "Internal server error: [error details]"
}
```

## Features

### 1. Pagination
- Hỗ trợ phân trang với `page` và `pageSize`
- Trả về thông tin phân trang chi tiết (`totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage`)

### 2. Search Functionality
- Tìm kiếm trong các trường: `Title`, `Description`, `Question`
- Không phân biệt hoa thường
- Tìm kiếm dạng LIKE (Contains)

### 3. Recent Answers
- Hiển thị 2 câu trả lời mới nhất cho mỗi câu hỏi
- Sắp xếp theo `CreatedAt` giảm dần
- Bao gồm thông tin người trả lời và câu trả lời cha (nếu có)

### 4. Statistics
- `totalAnswers`: Tổng số câu trả lời cho câu hỏi
- `totalLikes`: Tổng số lượt thích cho câu hỏi

## Database Operations

API sử dụng Entity Framework Core với các Include operations:
- `ForumQuestion.AskByNavigation` (User)
- `ForumQuestion.AnswerQuestions.AnswerByNavigation` (User)
- `ForumQuestion.QuestionLikes`

## Performance Considerations

1. **Pagination**: Sử dụng `Skip()` và `Take()` để giới hạn kết quả
2. **Eager Loading**: Chỉ load các navigation properties cần thiết
3. **Search Optimization**: Tìm kiếm được thực hiện trên database level
4. **Ordering**: Sắp xếp theo `CreatedAt` để hiển thị câu hỏi mới nhất trước

## Usage Examples

### Lấy trang đầu tiên với 10 câu hỏi
```http
GET /api/ForumQuestions/forum/1/paginated
```

### Tìm kiếm với từ khóa "API"
```http
GET /api/ForumQuestions/forum/1/paginated?search=API
```

### Phân trang với kích thước trang tùy chỉnh
```http
GET /api/ForumQuestions/forum/1/paginated?page=2&pageSize=20
```

### Kết hợp tìm kiếm và phân trang
```http
GET /api/ForumQuestions/forum/1/paginated?page=1&pageSize=5&search=database
```
