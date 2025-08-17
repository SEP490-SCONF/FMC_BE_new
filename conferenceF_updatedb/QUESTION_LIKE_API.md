# Forum Question Like API Documentation

## Overview
Hệ thống Like cho ForumQuestion cho phép users like/unlike các câu hỏi trong forum và xem thống kê likes.

## API Endpoints

### 1. Toggle Like/Unlike Question
**POST** `/api/ForumQuestions/like`

Toggle like/unlike một forum question. Nếu user đã like thì sẽ unlike, nếu chưa like thì sẽ like.

**Request Body:**
```json
{
  "fqId": 1,
  "userId": 123
}
```

**Response:**
```json
{
  "fqId": 1,
  "isLiked": true,
  "totalLikes": 8,
  "message": "Question liked successfully"
}
```

### 2. Get Question Like Statistics
**GET** `/api/ForumQuestions/{fqId}/likes?currentUserId={userId}`

Lấy thống kê likes của một forum question cụ thể.

**Parameters:**
- `fqId` (path): ID của forum question
- `currentUserId` (query, optional): ID của user hiện tại để check like status

**Response:**
```json
{
  "fqId": 1,
  "totalLikes": 8,
  "isLikedByCurrentUser": true,
  "recentLikes": [
    {
      "likeId": 201,
      "fqId": 1,
      "likedBy": 123,
      "createdAt": "2024-01-15T10:30:00Z",
      "likerName": "John Doe",
      "likerEmail": "john@example.com",
      "questionTitle": "How to implement authentication?"
    }
  ]
}
```

### 3. Get User's Question Likes
**GET** `/api/ForumQuestions/user/{userId}/likes`

Lấy tất cả forum questions mà user đã like.

**Response:**
```json
[
  {
    "likeId": 201,
    "fqId": 1,
    "likedBy": 123,
    "createdAt": "2024-01-15T10:30:00Z",
    "likerName": "John Doe",
    "likerEmail": "john@example.com",
    "questionTitle": "How to implement authentication in ASP.NET Core?"
  }
]
```

### 4. Remove Question Like
**DELETE** `/api/ForumQuestions/{fqId}/likes/{userId}`

Xóa like của user cho forum question cụ thể.

**Response:**
```json
{
  "fqId": 1,
  "totalLikes": 7,
  "message": "Like removed successfully"
}
```

### 5. Get Popular Questions
**GET** `/api/ForumQuestions/popular?forumId={id}&limit={number}`

Lấy các forum questions phổ biến nhất (nhiều likes nhất).

**Parameters:**
- `forumId` (query, optional): Filter theo forum
- `limit` (query, default: 10): Số lượng questions trả về

**Response:**
```json
[
  {
    "fqId": 1,
    "title": "How to implement authentication?",
    "description": "I need help with implementing authentication...",
    "askerName": "John Doe",
    "createdAt": "2024-01-15T10:30:00Z",
    "totalLikes": 15,
    "forumId": 1
  }
]
```

## JavaScript Services

```javascript
// Question Like Services

// Toggle like/unlike question
export const toggleQuestionLike = async (fqId, userId) => {
    const data = {
        fqId: fqId,
        userId: userId
    };
    return apiService.post("/ForumQuestions/like", data);
};

// Get likes for specific question
export const getQuestionLikes = async (fqId, currentUserId = null) => {
    const queryParams = new URLSearchParams();
    if (currentUserId) {
        queryParams.append('currentUserId', currentUserId);
    }
    
    const url = `/ForumQuestions/${fqId}/likes${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
    return apiService.get(url);
};

// Get all question likes by a user
export const getUserQuestionLikes = async (userId) => {
    return apiService.get(`/ForumQuestions/user/${userId}/likes`);
};

// Remove question like (alternative to toggle)
export const removeQuestionLike = async (fqId, userId) => {
    return apiService.delete(`/ForumQuestions/${fqId}/likes/${userId}`);
};

// Get popular questions (most liked)
export const getPopularQuestions = async (forumId = null, limit = 10) => {
    const queryParams = new URLSearchParams({
        limit: limit,
        ...(forumId && { forumId: forumId })
    });
    
    return apiService.get(`/ForumQuestions/popular?${queryParams}`);
};

// Check if user liked specific question
export const checkUserLikedQuestion = async (fqId, userId) => {
    try {
        const response = await getQuestionLikes(fqId, userId);
        return response.data.isLikedByCurrentUser;
    } catch (error) {
        throw error;
    }
};

// Get like statistics for multiple questions
export const getMultipleQuestionLikes = async (fqIds, currentUserId = null) => {
    const promises = fqIds.map(id => getQuestionLikes(id, currentUserId));
    return Promise.allSettled(promises);
};

// Helper function for UI
export const formatQuestionLikeCount = (count) => {
    if (count === 0) return "No likes";
    if (count === 1) return "1 like";
    return `${count} likes`;
};

// Export question like services group
export const questionLikeServices = {
    toggle: toggleQuestionLike,
    get: getQuestionLikes,
    getUserLikes: getUserQuestionLikes,
    remove: removeQuestionLike,
    getPopular: getPopularQuestions,
    checkUserLiked: checkUserLikedQuestion,
    getMultiple: getMultipleQuestionLikes,
    formatCount: formatQuestionLikeCount
};
```

## Usage Examples

### React Component Example

```jsx
import { toggleQuestionLike, getQuestionLikes } from './services/ForumService';

const QuestionComponent = ({ question, currentUserId }) => {
  const [likeStats, setLikeStats] = useState({
    totalLikes: 0,
    isLikedByCurrentUser: false
  });

  useEffect(() => {
    loadLikeStats();
  }, [question.fqId]);

  const loadLikeStats = async () => {
    try {
      const response = await getQuestionLikes(question.fqId, currentUserId);
      setLikeStats(response.data);
    } catch (error) {
      console.error('Error loading like stats:', error);
    }
  };

  const handleToggleLike = async () => {
    try {
      const response = await toggleQuestionLike(question.fqId, currentUserId);
      setLikeStats({
        totalLikes: response.data.totalLikes,
        isLikedByCurrentUser: response.data.isLiked
      });
    } catch (error) {
      console.error('Error toggling like:', error);
    }
  };

  return (
    <div className="question-item">
      <h3>{question.title}</h3>
      <p>{question.description}</p>
      <div className="question-actions">
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
  <div class="question-item">
    <h3>{{ question.title }}</h3>
    <p>{{ question.description }}</p>
    <div class="question-actions">
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
import { toggleQuestionLike, getQuestionLikes } from '@/services/ForumService';

export default {
  props: ['question', 'currentUserId'],
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
        const response = await getQuestionLikes(this.question.fqId, this.currentUserId);
        this.likeStats = response.data;
      } catch (error) {
        console.error('Error loading like stats:', error);
      }
    },
    async toggleLike() {
      try {
        const response = await toggleQuestionLike(this.question.fqId, this.currentUserId);
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

### Popular Questions Display

```jsx
import { getPopularQuestions } from './services/ForumService';

const PopularQuestionsComponent = ({ forumId }) => {
  const [popularQuestions, setPopularQuestions] = useState([]);

  useEffect(() => {
    loadPopularQuestions();
  }, [forumId]);

  const loadPopularQuestions = async () => {
    try {
      const response = await getPopularQuestions(forumId, 5);
      setPopularQuestions(response.data);
    } catch (error) {
      console.error('Error loading popular questions:', error);
    }
  };

  return (
    <div className="popular-questions">
      <h2>🔥 Popular Questions</h2>
      {popularQuestions.map(question => (
        <div key={question.fqId} className="popular-question-item">
          <h4>{question.title}</h4>
          <p>by {question.askerName}</p>
          <span className="likes-count">❤️ {question.totalLikes}</span>
        </div>
      ))}
    </div>
  );
};
```

## Features

✅ **Toggle Like/Unlike**: Một API cho cả like và unlike questions  
✅ **Real-time Statistics**: Cập nhật số lượng likes ngay lập tức  
✅ **User Status Check**: Kiểm tra user đã like question hay chưa  
✅ **Popular Questions**: Lấy questions được like nhiều nhất  
✅ **User Activity**: Xem tất cả question likes của user  
✅ **Validation**: Kiểm tra question và user tồn tại  
✅ **Error Handling**: Xử lý lỗi đầy đủ  
✅ **Database Consistency**: Sử dụng Entity Framework Context

## Database Operations

- **Entity Framework Core**: Sử dụng Include cho navigation properties
- **Async Operations**: Tất cả operations đều async/await
- **Transaction Safety**: Sử dụng SaveChangesAsync() cho consistency
- **Performance**: Query optimization với proper indexing

## Error Responses

### 404 Not Found
```json
{
  "message": "Forum question with ID 999 not found."
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

## Integration with Existing Services

Các services này tích hợp hoàn hảo với các ForumQuestion services hiện có:

```javascript
// Combined usage example
const questionWithLikeStats = async (fqId, currentUserId) => {
  const [questionResponse, likeResponse] = await Promise.all([
    getForumQuestionsById(fqId),
    getQuestionLikes(fqId, currentUserId)
  ]);
  
  return {
    ...questionResponse.data,
    likeStats: likeResponse.data
  };
};
```
