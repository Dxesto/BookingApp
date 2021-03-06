﻿using AutoMapper;
using BookingApp.Data.Models;
using BookingApp.DTOs;
using BookingApp.DTOs.Folder;
using BookingApp.DTOs.Resource;
using BookingApp.DTOs.Statistics;
using BookingApp.Entities.Statistics;
using BookingApp.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace BookingApp.Services
{
    public class MapperService : IMapperService
    {
        readonly IMapper autoMapperInstance;

        public MapperService()
        {
            autoMapperInstance = new Mapper(new MapperConfiguration(cfg => {

                cfg.CreateMap<Resource, ResourceBaseDto>().ReverseMap();
                cfg.CreateMap<Resource, ResourceBriefDto>().ReverseMap();
                cfg.CreateMap<Resource, ResourceDetailedDto>().ReverseMap();
                cfg.CreateMap<Resource, ResourceMaxDto>().ReverseMap();

                cfg.CreateMap<Folder, FolderBaseDto>();
                cfg.CreateMap<Folder, FolderMinimalDto>().ReverseMap();

                cfg.CreateMap<Rule, RuleBasicDTO>();
                cfg.CreateMap<Rule, RuleAdminDTO>();
                cfg.CreateMap<Rule, RuleDetailedDTO>().ReverseMap();

                cfg.CreateMap<ApplicationUser, AuthRegisterDto>().ReverseMap().ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));
                cfg.CreateMap<UserMinimalDto, ApplicationUser>().ReverseMap();
                cfg.CreateMap<UserUpdateDTO, ApplicationUser>().ReverseMap();

                cfg.CreateMap<BookingsStats, BookingStatsDTO>();
                cfg.CreateMap<ResourceStats, ResourceStatsDTO>();
                cfg.CreateMap<ResourceStats, ResourceStatsBriefDTO>();
            }));
        }

        /// <summary>
        /// Internal wrapper.
        /// </summary>
        public TDestination Map<TDestination>(object source) => (TDestination)Map(source, source.GetType(), typeof(TDestination));

        /// <summary>
        /// Internal wrapper.
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource source) => (TDestination)Map(source, typeof(TSource), typeof(TDestination));

        /// <summary>
        /// Internal wrapper.
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) => (TDestination)Map(source, destination, typeof(TSource), typeof(TDestination));

        /// <summary>
        /// Automapper wrapper.
        /// </summary>
        public object Map(object source, Type sourceType, Type destinationType) => autoMapperInstance.Map(source, sourceType, destinationType);

        /// <summary>
        /// Automapper wrapper.
        /// </summary>
        public object Map(object source, object destination, Type sourceType, Type destinationType) => autoMapperInstance.Map(source, destination, sourceType, destinationType);
    }
}