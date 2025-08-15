# Forum Management System - Controller Documentation

## Tổng quan
Hệ thống quản lý Forum đã được tạo với 2 controllers chính cho việc quản lý Forum và Forum Questions.

## 📁 Files đã tạo

### DTOs (Data Transfer Objects)
1. **ForumDto.cs** - DTOs cho Forum
   - `ForumDto`: Hiển thị thông tin forum
   - `ForumCreateDto`: Tạo forum mới
   - `ForumUpdateDto`: Cập nhật forum

2. **ForumQuestionDto.cs** - DTOs cho Forum Questions
   - `ForumQuestionDto`: Hiển thị thông tin câu hỏi
   - `ForumQuestionCreateDto`: Tạo câu hỏi mới
   - `ForumQuestionUpdateDto`: Cập nhật câu hỏi
   - `ForumQuestionSummaryDto`: Tóm tắt câu hỏi

### Controllers
1. **ForumsController.cs** - Quản lý Forums
2. **ForumQuestionsController.cs** - Quản lý Forum Questions

## 🚀 API Endpoints

### ForumsController

#### Quản lý Forum
- `GET /api/Forums` - Lấy tất cả forums
- `GET /api/Forums/{id}` - Lấy forum theo ID
- `GET /api/Forums/conference/{conferenceId}` - Lấy forum theo conference ID
- `POST /api/Forums` - Tạo forum mới
- `PUT /api/Forums/{id}` - Cập nhật forum
- `DELETE /api/Forums/{id}` - Xóa forum

#### Chức năng mở rộng
- `GET /api/Forums/{id}/questions-summary` - Tóm tắt câu hỏi trong forum
- `POST /api/Forums/{id}/activate` - Kích hoạt forum
- `GET /api/Forums/search?title=keyword&conferenceId=1` - Tìm kiếm forum

### ForumQuestionsController

#### Quản lý Questions
- `GET /api/ForumQuestions` - Lấy tất cả câu hỏi
- `GET /api/ForumQuestions/{id}` - Lấy câu hỏi theo ID
- `GET /api/ForumQuestions/forum/{forumId}` - Lấy câu hỏi theo forum ID
- `GET /api/ForumQuestions/user/{userId}` - Lấy câu hỏi theo user ID
- `POST /api/ForumQuestions` - Tạo câu hỏi mới
- `PUT /api/ForumQuestions/{id}` - Cập nhật câu hỏi
- `DELETE /api/ForumQuestions/{id}` - Xóa câu hỏi

#### Chức năng mở rộng
- `GET /api/ForumQuestions/search?title=keyword&forumId=1&userId=1` - Tìm kiếm câu hỏi
- `GET /api/ForumQuestions/{id}/summary` - Chi tiết đầy đủ câu hỏi

## 🔧 Tính năng chính

### ForumsController
✅ **CRUD Operations**: Create, Read, Update, Delete forums
✅ **Conference Integration**: Lấy forum theo conference ID
✅ **Validation**: Kiểm tra conference tồn tại, forum unique per conference
✅ **Safety Checks**: Không cho xóa forum có câu hỏi
✅ **Search**: Tìm kiếm theo title và conference ID
✅ **Statistics**: Đếm số câu hỏi trong forum

### ForumQuestionsController
✅ **CRUD Operations**: Create, Read, Update, Delete questions
✅ **Multi-level Navigation**: Forum, User, Answer, Like integration
✅ **Rich DTOs**: Bao gồm thông tin asker, forum, statistics
✅ **Advanced Search**: Tìm kiếm trong title, description, question content
✅ **User Filtering**: Lấy câu hỏi theo user
✅ **Summary Views**: Tóm tắt với answers và likes
✅ **Recent Activity**: Sắp xếp theo thời gian mới nhất

## 🔗 Dependencies sử dụng

### Repository Interfaces
- `IForumRepository`
- `IForumQuestionRepository`
- `IConferenceRepository`
- `IUserRepository`
- `IAnswerQuestionRepository`
- `IQuestionLikeRepository`

### Supporting Services
- `AutoMapper` (đã inject nhưng chưa sử dụng)
- Entity Framework Core
- ASP.NET Core MVC

## 🛡️ Error Handling
- Comprehensive try-catch blocks
- Meaningful error messages
- HTTP status codes chuẩn
- Validation với Data Annotations

## 🔍 Data Flow
1. **Request** → Controller
2. **Validation** → ModelState & Business rules
3. **Repository** → Database operations
4. **Mapping** → Entity ↔ DTO
5. **Response** → JSON with proper HTTP codes

## 📋 Validation Rules

### Forum Creation
- Conference ID phải tồn tại
- Title: 5-200 characters
- Mỗi conference chỉ có 1 forum

### Question Creation
- Forum ID phải tồn tại
- User ID phải tồn tại
- Title: 5-200 characters
- Description: 10-1000 characters
- Question: 10-2000 characters

## 🎯 Best Practices đã áp dụng
- ✅ Async/await pattern
- ✅ Repository pattern
- ✅ DTO pattern
- ✅ Separation of concerns
- ✅ Error handling
- ✅ Input validation
- ✅ RESTful API design
- ✅ Dependency injection

## 🚀 Cách sử dụng

### Tạo Forum cho Conference
```json
POST /api/Forums
{
  "conferenceId": 1,
  "title": "Q&A Session for AI Conference 2024"
}
```

### Đặt câu hỏi trong Forum
```json
POST /api/ForumQuestions
{
  "askBy": 123,
  "forumId": 1,
  "title": "How to implement neural networks?",
  "description": "I'm new to AI and want to understand neural networks",
  "question": "Can someone explain the basic concepts of neural networks and how to implement them in Python?"
}
```

### Tìm kiếm câu hỏi
```
GET /api/ForumQuestions/search?title=neural&forumId=1
```

Hệ thống Forum đã sẵn sàng để tích hợp và sử dụng! 🎉
