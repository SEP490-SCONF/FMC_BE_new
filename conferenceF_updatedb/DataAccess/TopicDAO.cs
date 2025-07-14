using BussinessObject.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TopicDAO
    {
        private readonly ConferenceFTestContext _context;

        public TopicDAO(ConferenceFTestContext context)
        {
            _context = context;
        }

        // Get all topics
        public async Task<IEnumerable<Topic>> GetAllTopics()
        {
            try
            {
                return await _context.Topics
                                     .AsNoTracking()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving all topics.", ex);
            }
        }
        public async Task<IEnumerable<Topic>> GetTopicsByConferenceId(int conferenceId)
        {
            // Truy vấn các Topic mà có ít nhất một Conference trong danh sách Conferences của nó
            // có ConferenceId khớp với conferenceId được cung cấp.
            return await _context.Topics
                                   .Where(topic => topic.Conferences.Any(c => c.ConferenceId == conferenceId))
                                   .ToListAsync();
        }

        // Get topic by ID
        public async Task<Topic> GetTopicById(int topicId)
        {
            try
            {
                return await _context.Topics
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(t => t.TopicId == topicId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving topic with ID {topicId}.", ex);
            }
        }

        // Get topics by conference ID
        //public async Task<IEnumerable<Topic>> GetTopicsByConferenceId(int conferenceId)
        //{
        //    try
        //    {
        //        return await _context.Topics
        //                             .Where(t => t.ConferenceId == conferenceId)
        //                             .AsNoTracking()
        //                             .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error occurred while retrieving topics for conference ID {conferenceId}.", ex);
        //    }
        //}

        // Add a new topic
        public async Task AddTopic(Topic topic)
        {
            try
            {
                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while adding a new topic.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding a new topic.", ex);
            }
        }

        // Update an existing topic
        public async Task UpdateTopic(Topic topic)
        {
            try
            {
                var existing = await _context.Topics.FindAsync(topic.TopicId);
                if (existing == null)
                    throw new Exception($"Topic with ID {topic.TopicId} not found.");

                _context.Entry(existing).CurrentValues.SetValues(topic);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while updating the topic.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating topic with ID {topic.TopicId}.", ex);
            }
        }

        // Delete a topic
        public async Task DeleteTopic(int topicId)
        {
            try
            {
                var topic = await _context.Topics.FindAsync(topicId);
                if (topic != null)
                {
                    _context.Topics.Remove(topic);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Topic with ID {topicId} not found for deletion.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Database error while deleting the topic.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting topic with ID {topicId}.", ex);
            }
        }

        public async Task<IEnumerable<Topic>> GetTopicsByIdsAsync(List<int> topicIds)
        {
            try
            {
                return await _context.Topics
                                     .Where(t => topicIds.Contains(t.TopicId))
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving topics by IDs.", ex);
            }
        }

    }
}
