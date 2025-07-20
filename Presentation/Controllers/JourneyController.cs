using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Azure.Core;
using DocumentFormat.OpenXml.VariantTypes;
using DTO.DTO.Journey;
using DTO.WebApiDTO.Journey;
using DTO.WebApiDTO.User;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extentions;
using Presentation.Utilities;
using Presentation.Validators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class JourneyController : ControllerBase
    {
        private readonly IJourneyServices _journeyServices;
        private readonly IMapper _mapper;
        private readonly IJWTUtilities _jwt;
        private IValidator<JourneyShareRequestDtoApi> _journeyShareValidator;
        private IValidator<AddJourneyRequestDtoApi> _addjourneyValidator;
        private IValidator<JourneyFilterRequestDtoApi> _jounreyFilterValidator;
        private IValidator<MonthlyRouteDistanceDtoApi> _monthlyJourniesValidator;
        public JourneyController(IJourneyServices journeyService, IMapper mapper, IJWTUtilities jwt,
        IValidator<JourneyShareRequestDtoApi> journeyShareValidator, IValidator<AddJourneyRequestDtoApi> addjourneyValidator, 
        IValidator<JourneyFilterRequestDtoApi> jounreyFilterValidator, IValidator<MonthlyRouteDistanceDtoApi> monthlyJourniesValidator)
        {
            _journeyServices = journeyService;
            _mapper = mapper;
            _jwt = jwt;
            _journeyShareValidator = journeyShareValidator;
            _addjourneyValidator = addjourneyValidator;
            _jounreyFilterValidator = jounreyFilterValidator;
            _monthlyJourniesValidator = monthlyJourniesValidator;
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Guid>> AddJourney([FromBody] AddJourneyRequestDtoApi request)
        {
              var result = await _addjourneyValidator.ValidateAsync(request);

                result.AddToModelState(ModelState);
                if (!result.IsValid)
                {
                    result.AddToModelState(ModelState);
                    ValidationExtensions.CheckModelState(this.ModelState); result.AddToModelState(ModelState);
                }
                var claimsPrincipal = HttpContext.User;
                var jwtToken = new JwtSecurityTokenHandler()
                    .ReadJwtToken(HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "") ?? string.Empty);

                var user = _jwt.GetUserFromJWTToken(jwtToken);
                var oid = await _journeyServices.AddJourneyAsync(_mapper.Map<AddJourneyRequestDto>(request, opt => opt.AfterMap((o, dest) =>
                {
                    dest.UserId = Guid.Parse(user.Id.ToString());
                })));
                return Ok(oid);
            
        }

        [Authorize(Roles = "User")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<JourneyDtoApi>> GetJourneyById(Guid id)
        {

            var journey = await _journeyServices.GetJourneyByIdAsync(id);

            return Ok(_mapper.Map<JourneyDtoApi>(journey));
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<JourneyDtoApi>>> GetAllJourneys()
        {
            var claimsPrincipal = HttpContext.User;
            var jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "") ?? string.Empty);

            var user = _jwt.GetUserFromJWTToken(jwtToken);
            var journeys = await _journeyServices.GetAllJourneysForUserAsync(Guid.Parse(user.Id.ToString()));
            return Ok(_mapper.Map<List<JourneyDtoApi>>(journeys));
        }

        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteJourney(Guid id)
        {
            await _journeyServices.DeleteJourneyAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "User")]
        [HttpPost("{id}/share")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]

        public async Task<IActionResult> ShareJourney(Guid id, [FromBody] JourneyShareRequestDtoApi request)
        {
            var result = await _journeyShareValidator.ValidateAsync(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                ValidationExtensions.CheckModelState(this.ModelState); result.AddToModelState(ModelState);
            }
            var claimsPrincipal = HttpContext.User;
            var jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "") ?? string.Empty);

            var user = _jwt.GetUserFromJWTToken(jwtToken);
            var response = await _journeyServices.ShareJourneyAsync(id,Guid.Parse(user.Id.ToString()),_mapper.Map<JourneyShareRequestDto>(request));
            return Ok(_mapper.Map<JourneyShareResponseDtoApi>(response));
        }



        [Authorize(Roles = "User")]
        [HttpPost("{id}/public-link")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<IActionResult> GeneratePublicLink(Guid id)
        {
            var claimsPrincipal = HttpContext.User;
            var jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "") ?? string.Empty);

            var user = _jwt.GetUserFromJWTToken(jwtToken);
            var response = await _journeyServices.GeneratePublicLinkAsync(id, Guid.Parse(user.Id.ToString()));
            return Ok(_mapper.Map<PublicJourneyLinkResponseDtoApi>(response));
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}/public-link")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<IActionResult> RevokePublicLink(Guid id)
        {
            var claimsPrincipal = HttpContext.User;
            var jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "") ?? string.Empty);

            var user = _jwt.GetUserFromJWTToken(jwtToken);
            await _journeyServices.RevokePublicLinkAsync(id, Guid.Parse(user.Id.ToString()));
            return NoContent();
        }

        [Authorize(Roles = "User")]
        [HttpGet("public/{token}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<IActionResult> GetPublicJourneyByToken(string token)
        {
            var journeyDto = await _journeyServices.GetPublicJourneyByTokenAsync(token);

            return Ok(_mapper.Map<JourneyPublicLinkDtoApi>(journeyDto));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/journeys")]
        public async Task<IActionResult> GetJourneys([FromQuery] JourneyFilterRequestDtoApi filter)
        {
            var result = await _jounreyFilterValidator.ValidateAsync(filter);

            result.AddToModelState(ModelState);
            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                ValidationExtensions.CheckModelState(this.ModelState); result.AddToModelState(ModelState);
            }
            var response = await _journeyServices.GetJourniesByFilter(_mapper.Map<JourneyFilterRequestDto>(filter));

            return Ok(_mapper.Map<JourneyFilterResponseDtoApi>(response));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/monthlyRouteDistances")]
        public async Task<IActionResult> GetMonthlyRouteDistances([FromQuery]MonthlyRouteDistanceDtoApi filter)
        {
            var result = await _monthlyJourniesValidator.ValidateAsync(filter);

            result.AddToModelState(ModelState);
            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                ValidationExtensions.CheckModelState(this.ModelState); result.AddToModelState(ModelState);
            }
            var response =  await _journeyServices.GetMonthlyDistancesAsync(_mapper.Map<MonthlyRouteDistanceDto>(filter));
            return Ok(_mapper.Map<List<MonthlyRouteDistanceResponseDtoApi>>(response));
        }
    }
}
