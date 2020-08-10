﻿using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.WebAPI.Common.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : BaseController
    {
        private readonly IPlexDownloadService _plexDownloadService;
        private readonly IMapper _mapper;

        public DownloadController(IPlexDownloadService plexDownloadService, IMapper mapper) : base(mapper)
        {
            _plexDownloadService = plexDownloadService;
            _mapper = mapper;
        }

        // GET: api/<DownloadController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<IEnumerable<DownloadTaskDTO>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Get()
        {
            var result = await _plexDownloadService.GetAllDownloadsAsync();
            if (result.IsFailed)
            {
                return InternalServerError(result);
            }
            var mapResult = _mapper.Map<List<DownloadTaskDTO>>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        // Get api/<DownloadController>/movie/{plexMovieId:int}?plexAccountId={id}
        [HttpPost("movie")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Post([FromBody] DownloadMovieDTO downloadMovie)
        {
            int plexMovieId = downloadMovie.PlexMovieId;
            int plexAccountId = downloadMovie.PlexAccountId;

            if (plexMovieId <= 0) { return BadRequest(plexMovieId, nameof(plexMovieId)); }
            if (plexAccountId <= 0) { return BadRequest(plexAccountId, nameof(plexAccountId)); }

            var result = await _plexDownloadService.DownloadMovieAsync(plexAccountId, plexMovieId);

            if (result.IsFailed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        // DELETE api/<DownloadController>/5
        [HttpDelete("{downloadTaskId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Delete(int downloadTaskId)
        {
            if (downloadTaskId <= 0) { return BadRequest(downloadTaskId, nameof(downloadTaskId)); }

            var result = await _plexDownloadService.DeleteDownloadsAsync(downloadTaskId);
            if (result.IsFailed)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}