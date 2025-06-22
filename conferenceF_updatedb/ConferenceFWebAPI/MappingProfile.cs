﻿using AutoMapper;
using BussinessObject.Entity;
using ConferenceFWebAPI.DTOs;

namespace ConferenceFWebAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Conference, ConferenceDTO>();
            CreateMap<ConferenceDTO, Conference>();
            CreateMap<Topic, TopicDTO>();
            CreateMap<TopicDTO, Topic>();

        }
    }
}
