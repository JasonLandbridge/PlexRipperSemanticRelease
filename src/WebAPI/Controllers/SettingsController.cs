﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using System;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : BaseController
    {
        private readonly ISettingsService _settingsService;
        private readonly IMapper _mapper;

        public SettingsController(ISettingsService settingsService, IMapper mapper) : base(mapper)
        {
            _settingsService = settingsService;
            _mapper = mapper;
        }

        // GET api/<SettingsController>/activeaccount/
        [HttpGet("activeaccount/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _settingsService.GetActivePlexAccountAsync();
                if (result.IsFailed)
                {
                    return InternalServerError(result);
                }
                var mapResult = _mapper.Map<PlexAccountDTO>(result.Value);
                return Ok(Result.Ok(mapResult));

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PUT api/<SettingsController>/activeaccount/5
        [HttpPut("activeaccount/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Put(int id)
        {
            if (id <= 0) { return BadRequestInvalidId(); }

            try
            {
                Log.Debug($"Setting the active plex account to {id}");

                var result = await _settingsService.SetActivePlexAccountAsync(id);
                if (result.IsFailed)
                {
                    return BadRequest(result);
                }

                var mapResult = _mapper.Map<PlexAccountDTO>(result.Value);
                return Ok(Result.Ok(mapResult));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}