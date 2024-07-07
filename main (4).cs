/******************************************************************************

                            Online C# Compiler.
                Code, Compile, Run and Debug C# program online.
Write your code in this editor and press "Run" button to execute it.

*******************************************************************************/

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfficeOpenXml; // For Excel export
using System.Linq;

namespace EmployeeManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GetAll API for EmployeeBasicDetails
        [HttpGet("GetAllBasicDetails")]
        public async Task<IActionResult> GetAllBasicDetails([FromQuery] FilterCriteria filterCriteria)
        {
            var result = await _employeeService.GetAllEmployeeBasicDetailsAsync(filterCriteria);
            return Ok(result);
        }

        // GetAll API for EmployeeAdditionalDetails
        [HttpGet("GetAllAdditionalDetails")]
        public async Task<IActionResult> GetAllAdditionalDetails([FromQuery] FilterCriteria filterCriteria)
        {
            var result = await _employeeService.GetAllEmployeeAdditionalDetailsAsync(filterCriteria);
            return Ok(result);
        }

        // API to demonstrate the use of MakePostRequest
        [HttpPost("CreateEmployee")]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDTO employeeDTO)
        {
            var result = await _employeeService.CreateEmployeeAsync(employeeDTO);
            return Ok(result);
        }

        // API to demonstrate the use of MakeGetRequest
        [HttpGet("GetEmployeeById/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var result = await _employeeService.GetEmployeeByIdAsync(id);
            return Ok(result);
        }

        // Export an Excel containing all basic details + additional details
        [HttpGet("ExportEmployeeDetails")]
        public async Task<IActionResult> ExportEmployeeDetails()
        {
            var basicDetails = await _employeeService.GetAllEmployeeBasicDetailsAsync(new FilterCriteria());
            var additionalDetails = await _employeeService.GetAllEmployeeAdditionalDetailsAsync(new FilterCriteria());

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("EmployeeDetails");
                var data = (from basic in basicDetails.Records
                            join additional in additionalDetails.Records
                            on basic.UId equals additional.BasicDetailsUId
                            select new
                            {
                                basic.UId,
                                basic.Name,
                                basic.Email,
                                additional.Address,
                                additional.PhoneNumber
                            }).ToList();

                worksheet.Cells["A1"].LoadFromCollection(data, true);
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"EmployeeDetails-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // GetEmployeeAdditionalDetailsByBasicDetailsUId
        [HttpGet("GetAdditionalDetailsByBasicUId/{basicUId}")]
        public async Task<IActionResult> GetAdditionalDetailsByBasicUId(int basicUId)
        {
            var result = await _employeeService.GetEmployeeAdditionalDetailsByBasicUIdAsync(basicUId);
            return Ok(result);
        }
    }

    // Example FilterCriteria class
    public class FilterCriteria
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public string FilterAttribute { get; set; }
        public string FilterValue { get; set; }
    }

    // Example DTOs
    public class EmployeeDTO
    {
        public int UId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    // Example Employee Service Interface
    public interface IEmployeeService
    {
        Task<PagedResult<EmployeeBasicDetails>> GetAllEmployeeBasicDetailsAsync(FilterCriteria filterCriteria);
        Task<PagedResult<EmployeeAdditionalDetails>> GetAllEmployeeAdditionalDetailsAsync(FilterCriteria filterCriteria);
        Task<EmployeeDTO> CreateEmployeeAsync(EmployeeDTO employeeDTO);
        Task<EmployeeDTO> GetEmployeeByIdAsync(int id);
        Task<EmployeeAdditionalDetails> GetEmployeeAdditionalDetailsByBasicUIdAsync(int basicUId);
    }

    // Example PagedResult class
    public class PagedResult<T>
    {
        public List<T> Records { get; set; }
        public int TotalRecords { get; set; }
    }
}
