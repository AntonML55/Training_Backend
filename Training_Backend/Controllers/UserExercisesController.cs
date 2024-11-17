using Microsoft.AspNetCore.Mvc;
using System.Data;
using Training_Backend.Models;
using Microsoft.Data.SqlClient;


namespace Training_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserExercisesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _sqlDataSource;

        public UserExercisesController(IConfiguration configuration)
        {
            _sqlDataSource = configuration.GetConnectionString("TrainingDbCon");
        }
        
        [HttpGet("{startDate}-to-{endDate}")]
        public JsonResult GetUserExercises(DateOnly startDate, DateOnly endDate)
        {
            DataTable userExTable = new DataTable();
            DataTable exStatusesTable = new DataTable();
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(_sqlDataSource))
            {
                myCon.Open();

                using (SqlCommand myCommand = new SqlCommand("SP_GET_USER_EXERCISES", myCon))
                {
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.Parameters.AddWithValue("@EXERCISE_START_DATE", startDate);
                    myCommand.Parameters.AddWithValue("@EXERCISE_END_DATE", endDate);
                    myReader = myCommand.ExecuteReader();
                    userExTable.Load(myReader);
                    myReader.Close();
           
                }

                using (SqlCommand myCommand = new SqlCommand("SP_GET_EXERCISES_STATUSES", myCon))
                {
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myReader = myCommand.ExecuteReader();
                    exStatusesTable.Load(myReader);
                    myReader.Close();

                }

                myCon.Close();
            }

            var result = new
            {
                userEx = userExTable,
                exStatuses = exStatusesTable
            };

            return new JsonResult(result);
        }

        [HttpGet()]
        public JsonResult GetExercisesInfo()
        {
            DataTable exInfoTable = new DataTable();
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(_sqlDataSource))
            {
                myCon.Open();

                using (SqlCommand myCommand = new SqlCommand("SP_GET_EXERCISES_INFO", myCon))
                {
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myReader = myCommand.ExecuteReader();
                    exInfoTable.Load(myReader);
                    myReader.Close();

                }

                myCon.Close();
            }


            return new JsonResult(exInfoTable);
        }

        [HttpPut]
        public JsonResult UpdateExerciseStatus(UserExerciseStatusUpdate exStatusUpdate)
        {
            try
            {  
                using (SqlConnection myCon = new SqlConnection(_sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand("SP_UPDATE_EXERCISE_STATUS", myCon))
                    {
                        myCommand.CommandType = CommandType.StoredProcedure;
                        myCommand.Parameters.AddWithValue("@EXERCISE_ID", exStatusUpdate.EXERCISE_ID);
                        myCommand.Parameters.AddWithValue("@UPDATED_EXERCISE_STATUS", exStatusUpdate.UPDATED_EXERCISE_STATUS);
                        myCommand.ExecuteNonQuery();
                    }
                }

                return new JsonResult("Status updated successfully");
            }
            catch (Exception ex)
            {
                return new JsonResult($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public JsonResult Post(UserExerciseAdd userExAdd)
        {          
            try
            {
                using (SqlConnection myCon = new SqlConnection(_sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand("SP_ADD_USER_EXERCISE", myCon))
                    {
                        myCommand.CommandType = CommandType.StoredProcedure;
                        myCommand.Parameters.AddWithValue("@EXERCISE_NAME", userExAdd.EXERCISE_NAME);
                        myCommand.Parameters.AddWithValue("@EXERCISE_PROGRESS", userExAdd.EXERCISE_PROGRESS);
                        myCommand.Parameters.AddWithValue("@EXERCISE_STATUS", userExAdd.EXERCISE_STATUS);
                        myCommand.Parameters.AddWithValue("@EXERCISE_DATE", userExAdd.EXERCISE_DATE);
                        myCommand.ExecuteNonQuery();
                    }
                }

                return new JsonResult("Added successfully");
            }
            catch (Exception ex)
            {
                return new JsonResult($"Error: {ex.Message}");
            }
        }
    }
}
