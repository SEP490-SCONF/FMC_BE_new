# Answer Like API Documentation

## Overview
Hệ thống Like cho AnswerQuestion cho phép users like/unlike các câu trả lời và xem thống kê likes.

## API Endpoints

### 1. Toggle Like/Unlike Answer
**POST** `/api/AnswerQuestions/like`

Toggle like/unlike một answer. Nếu user đã like thì sẽ unlike, nếu chưa like thì sẽ like.

**Request Body:**
```json
{
  "answerId": 1,
  "userId": 123
}
```

**Response:**
```json
{
  "answerId": 1,
  "isLiked": true,
  "totalLikes": 5,
  "message": "Answer liked successfully"
}
```

### 2. Get Answer Like Statistics
**GET** `/api/AnswerQuestions/{answerId}/likes?currentUserId={userId}`

Lấy thống kê likes của một answer cụ thể.

**Parameters:**
- `answerId` (path): ID của answer
- `currentUserId` (query, optional): ID của user hiện tại để check like status

**Response:**
```json
{
  "answerId": 1,
  "totalLikes": 5,
  "isLikedByCurrentUser": true,
  "recentLikes": [
    {
      "likeId": 101,
      "answerId": 1,
      "likedBy": 123,
      "createdAt": "2024-01-15T10:30:00Z",
      "likerName": "John Doe",
      "likerEmail": "john@example.com"
    }
  ]
}
```

### 3. Get User's Likes
**GET** `/api/AnswerQuestions/user/{userId}/likes`

Lấy tất cả answers mà user đã like.

**Response:**
```json
[
  {
    "likeId": 101,
    "answerId": 1,
    "likedBy": 123,
    "createdAt": "2024-01-15T10:30:00Z",
    "likerName": "John Doe",
    "likerEmail": "john@example.com",
    "answerContent": "This is a great answer that explains..."
  }
]
```

### 4. Remove Like
**DELETE** `/api/AnswerQuestions/{answerId}/likes/{userId}`

Xóa like của user cho answer cụ thể.

**Response:**
```json
{
  "answerId": 1,
  "totalLikes": 4,
  "message": "Like removed successfully"
}
```

### 5. Get Popular Answers
**GET** `/api/AnswerQuestions/popular?forumQuestionId={id}&limit={number}`

Lấy các answers phổ biến nhất (nhiều likes nhất).

**Parameters:**
- `forumQuestionId` (query, optional): Filter theo forum question
- `limit` (query, default: 10): Số lượng answers trả về

**Response:**
```json
[
  {
    "answerId": 1,
    "answer": "This is the most popular answer...",
    "answererName": "John Doe",
    "createdAt": "2024-01-15T10:30:00Z",
    "totalLikes": 15,
    "fqId": 1
  }
]
```

## JavaScript Services

```javascript
// Answer Like Services

// Toggle like/unlike answer
export const toggleAnswerLike = async (answerId, userId) => {
    const data = {
        answerId: answerId,
        userId: userId
    };
    return apiService.post("/AnswerQuestions/like", data);
};

// Get likes for specific answer
export const getAnswerLikes = async (answerId, currentUserId = null) => {
    const queryParams = new URLSearchParams();
    if (currentUserId) {
        queryParams.append('currentUserId', currentUserId);
    }
    
    const url = `/AnswerQuestions/${answerId}/likes${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
    return apiService.get(url);
};

// Get all likes by a user
export const getUserLikes = async (userId) => {
    return apiService.get(`/AnswerQuestions/user/${userId}/likes`);
};

// Remove like (alternative to toggle)
export const removeAnswerLike = async (answerId, userId) => {
    return apiService.delete(`/AnswerQuestions/${answerId}/likes/${userId}`);
};

// Get popular answers (most liked)
export const getPopularAnswers = async (forumQuestionId = null, limit = 10) => {
    const queryParams = new URLSearchParams({
        limit: limit,
        ...(forumQuestionId && { forumQuestionId: forumQuestionId })
    });
    
    return apiService.get(`/AnswerQuestions/popular?${queryParams}`);
};

// Check if user liked specific answer
export const checkUserLikedAnswer = async (answerId, userId) => {
    try {
        const response = await getAnswerLikes(answerId, userId);
        return response.data.isLikedByCurrentUser;
    } catch (error) {
        throw error;
    }
};

// Helper function for UI
export const formatLikeCount = (count) => {
    if (count === 0) return "No likes";
    if (count === 1) return "1 like";
    return `${count} likes`;
};

// Export like services group
export const answerLikeServices = {
    toggle: toggleAnswerLike,
    get: getAnswerLikes,
    getUserLikes: getUserLikes,
    remove: removeAnswerLike,
    getPopular: getPopularAnswers,
    checkUserLiked: checkUserLikedAnswer,
    formatCount: formatLikeCount
};
```

## Usage Examples

### React Component Example

```jsx
import { toggleAnswerLike, getAnswerLikes } from './services/ForumService';

const AnswerComponent = ({ answer, currentUserId }) => {
  const [likeStats, setLikeStats] = useState({
    totalLikes: 0,
    isLikedByCurrentUser: false
  });

  useEffect(() => {
    loadLikeStats();
  }, [answer.answerId]);

  const loadLikeStats = async () => {
    try {
      const response = await getAnswerLikes(answer.answerId, currentUserId);
      setLikeStats(response.data);
    } catch (error) {
      console.error('Error loading like stats:', error);
    }
  };

  const handleToggleLike = async () => {
    try {
      const response = await toggleAnswerLike(answer.answerId, currentUserId);
      setLikeStats({
        totalLikes: response.data.totalLikes,
        isLikedByCurrentUser: response.data.isLiked
      });
    } catch (error) {
      console.error('Error toggling like:', error);
    }
  };

  return (
    <div className="answer-item">
      <p>{answer.answer}</p>
      <div className="answer-actions">
        <button 
          onClick={handleToggleLike}
          className={`like-button ${likeStats.isLikedByCurrentUser ? 'liked' : ''}`}
        >
          {likeStats.isLikedByCurrentUser ? '❤️' : '🤍'} {likeStats.totalLikes}
        </button>
      </div>
    </div>
  );
};
```

### Vue.js Example

```vue
<template>
  <div class="answer-item">
    <p>{{ answer.answer }}</p>
    <div class="answer-actions">
      <button 
        @click="toggleLike"
        :class="['like-button', { 'liked': likeStats.isLikedByCurrentUser }]"
      >
        {{ likeStats.isLikedByCurrentUser ? '❤️' : '🤍' }} {{ likeStats.totalLikes }}
      </button>
    </div>
  </div>
</template>

<script>
import { toggleAnswerLike, getAnswerLikes } from '@/services/ForumService';

export default {
  props: ['answer', 'currentUserId'],
  data() {
    return {
      likeStats: {
        totalLikes: 0,
        isLikedByCurrentUser: false
      }
    };
  },
  async mounted() {
    await this.loadLikeStats();
  },
  methods: {
    async loadLikeStats() {
      try {
        const response = await getAnswerLikes(this.answer.answerId, this.currentUserId);
        this.likeStats = response.data;
      } catch (error) {
        console.error('Error loading like stats:', error);
      }
    },
    async toggleLike() {
      try {
        const response = await toggleAnswerLike(this.answer.answerId, this.currentUserId);
        this.likeStats = {
          totalLikes: response.data.totalLikes,
          isLikedByCurrentUser: response.data.isLiked
        };
      } catch (error) {
        console.error('Error toggling like:', error);
      }
    }
  }
};
</script>
```

## Features

✅ **Toggle Like/Unlike**: Một API cho cả like và unlike  
✅ **Real-time Statistics**: Cập nhật số lượng likes ngay lập tức  
✅ **User Status Check**: Kiểm tra user đã like hay chưa  
✅ **Popular Answers**: Lấy answers được like nhiều nhất  
✅ **User Activity**: Xem tất cả likes của user  
✅ **Validation**: Kiểm tra answer và user tồn tại  
✅ **Error Handling**: Xử lý lỗi đầy đủ  

## Database Operations

- **Entity Framework Core**: Sử dụng Include cho navigation properties
- **Async Operations**: Tất cả operations đều async/await
- **Transaction Safety**: Sử dụng SaveChangesAsync() cho consistency
- **Performance**: Query optimization với proper indexing

## Error Responses

### 404 Not Found
```json
{
  "message": "Answer with ID 999 not found."
}
```

### 400 Bad Request
```json
{
  "message": "User with ID 999 not found."
}
```

### 500 Internal Server Error
```json
{
  "message": "Internal server error: [error details]"
}
```
