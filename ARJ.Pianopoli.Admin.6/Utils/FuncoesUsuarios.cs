//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNet.Identity;

//namespace ARJ.Pianopoli.Admin._6.Utils
//{
//    public class FuncoesUsuarios
//    {


//        [HttpGet]
//        public async Task<IActionResult> GetUser(string id)
//        {
//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null)
//            {
//                return NotFound();
//            }
//            return Ok(user);
//        }

//    }
//}
