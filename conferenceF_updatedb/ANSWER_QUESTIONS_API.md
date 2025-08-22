# Answer Questions API Documentation

## Overview

AnswerQuestionsController cung cấp các API để quản lý câu trả lời cho các câu hỏi trong forum với chức năng phân trang và tìm kiếm.

## Main Endpoint

### GET /api/AnswerQuestions/question/{forumQuestionId}/paginated

Lấy tất cả câu trả lời của một ForumQuestion với phân trang và tìm kiếm.

#### Parameters

**Path Parameters:**
- `forumQuestionId` (int, required): ID của forum question

**Query Parameters:**
- `page` (int, optional, default: 1): Trang hiện tại
- `pageSize` (int, optional, default: 10): Số lượng câu trả lời trên mỗi trang  
- `search` (string, optional): Từ khóa tìm kiếm trong nội dung câu trả lời

#### Request Example

```http
GET /api/AnswerQuestions/question/1/paginated?page=1&pageSize=5&search=solution
```

#### Response Structure

```json
{
  "answers": [
    {
      "answerId": 1,
      "fqId": 1,
      "answerBy": 123,
      "parentAnswerId": null,
      "answer": "This is a great solution to your problem...",
      "createdAt": "2024-01-15T10:30:00Z",
      "answererName": "John Doe",
      "answererEmail": "john@example.com",
      "forumQuestionTitle": "How to implement authentication?",
      "totalLikes": 5,
      "hasReplies": true,
      "totalReplies": 3,
      "parentAnswerText": null,
      "parentAnswererName": null
    },
    {
      "answerId": 2,
      "fqId": 1,
      "answerBy": 456,
      "parentAnswerId": 1,
      "answer": "I agree with the previous answer, but I would also add...",
      "createdAt": "2024-01-15T11:00:00Z",
      "answererName": "Jane Smith",
      "answererEmail": "jane@example.com", 
      "forumQuestionTitle": "How to implement authentication?",
      "totalLikes": 2,
      "hasReplies": false,
      "totalReplies": 0,
      "parentAnswerText": "This is a great solution to your problem...",
      "parentAnswererName": "John Doe"
    }
  ],
  "totalCount": 25,
  "currentPage": 1,
  "pageSize": 5,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "searchTerm": "solution",
  "forumQuestionId": 1,
  "forumQuestionTitle": "How to implement authentication?"
}
```

## All Endpoints

### 1. GET /api/AnswerQuestions
Lấy tất cả câu trả lời (không phân trang)

### 2. GET /api/AnswerQuestions/{id}
Lấy chi tiết một câu trả lời theo ID

### 3. GET /api/AnswerQuestions/question/{forumQuestionId}/paginated ⭐
**Endpoint chính** - Lấy câu trả lời với phân trang và tìm kiếm

### 4. GET /api/AnswerQuestions/question/{forumQuestionId}
Lấy tất cả câu trả lời của một forum question (không phân trang)

### 5. GET /api/AnswerQuestions/user/{userId}
Lấy tất cả câu trả lời của một user

### 6. POST /api/AnswerQuestions
Tạo câu trả lời mới

**Request Body:**
```json
{
  "fqId": 1,
  "answerBy": 123,
  "parentAnswerId": null,
  "answer": "Content of the answer"
}
```

### 7. PUT /api/AnswerQuestions/{id}
Cập nhật câu trả lời

**Request Body:**
```json
{
  "answerId": 1,
  "answer": "Updated content"
}
```

### 8. DELETE /api/AnswerQuestions/{id}
Xóa câu trả lời (không thể xóa nếu có replies)

### 9. GET /api/AnswerQuestions/search
Tìm kiếm câu trả lời với các filter

**Query Parameters:**
- `content`: Tìm kiếm trong nội dung
- `forumQuestionId`: Filter theo forum question
- `userId`: Filter theo user

## Features

### 🔍 Advanced Search
- Tìm kiếm trong nội dung câu trả lời
- Hỗ trợ tìm kiếm không phân biệt hoa thường
- Sử dụng LINQ Contains()

### 📄 Smart Pagination
- Hiển thị câu trả lời gốc (parent answers) trước
- Sau đó hiển thị câu trả lời con (replies)
- Sắp xếp theo thời gian tạo giảng dần

### 🔗 Reply System
- Hỗ trợ hệ thống reply (parent-child relationships)
- Hiển thị thông tin parent answer cho replies
- Đếm số lượng replies cho mỗi answer
- Không thể xóa answer có replies

### 📊 Rich Information
- Thông tin người trả lời (tên, email)
- Thống kê likes và replies
- Thông tin forum question
- Preview của parent answer (giới hạn 100 ký tự)

### 🛡️ Data Validation
- Kiểm tra forum question tồn tại
- Kiểm tra user tồn tại
- Kiểm tra parent answer hợp lệ (cùng forum question)
- Validation cho độ dài nội dung

## Database Operations

### Entity Framework Includes:
- `AnswerByNavigation` (User info)
- `ParentAnswer.AnswerByNavigation` (Parent answer user info)
- `AnswerLikes` (Like statistics)
- `InverseParentAnswer` (Reply counting)

### Optimized Queries:
- Eager loading để giảm database calls
- Pagination với Skip/Take
- Efficient filtering và search

## Error Handling

### 404 Not Found
```json
{
  "message": "Forum question with ID 999 not found."
}
```

### 400 Bad Request
```json
{
  "message": "Parent answer must belong to the same forum question."
}
```

### 500 Internal Server Error
```json
{
  "message": "Internal server error: [error details]"
}
```

## Usage Examples

### Lấy câu trả lời trang đầu
```http
GET /api/AnswerQuestions/question/1/paginated
```

### Tìm kiếm với từ khóa
```http
GET /api/AnswerQuestions/question/1/paginated?search=authentication
```

### Phân trang tùy chỉnh
```http
GET /api/AnswerQuestions/question/1/paginated?page=2&pageSize=20
```

### Tạo câu trả lời mới
```http
POST /api/AnswerQuestions
Content-Type: application/json

{
  "fqId": 1,
  "answerBy": 123,
  "answer": "Here's my solution to this problem..."
}
```

### Tạo reply cho câu trả lời
```http
POST /api/AnswerQuestions
Content-Type: application/json

{
  "fqId": 1,
  "answerBy": 456,
  "parentAnswerId": 1,
  "answer": "I agree with this solution, but would like to add..."
}
```

## Performance Considerations

1. **Efficient Pagination**: Sử dụng Skip/Take để giới hạn kết quả
2. **Eager Loading**: Include navigation properties để giảm N+1 queries
3. **Smart Ordering**: Parent answers hiển thị trước, tối ưu UX
4. **Search Optimization**: Database-level filtering
5. **Preview Text**: Giới hạn độ dài parent answer text
