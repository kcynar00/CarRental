using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Pages
{
    public class RegisterModel : PageModel
    {
        
        [BindProperty]
        public InputModel Input { get; set; }

        // Klasa opisująca pola formularza
        public class InputModel
        {
            [Required(ErrorMessage = "Imię i nazwisko jest wymagane")]
            [Display(Name = "Imię i Nazwisko")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email jest wymagany")]
            [EmailAddress(ErrorMessage = "Niepoprawny format adresu email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Hasło jest wymagane")]
            [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} znaków.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Powtórz hasło")]
            [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
            // Inicjalizacja pustego formularza przy wejściu na stronę
        }

        public void OnPost()
        {
            if (ModelState.IsValid)
            {
                // Tutaj w przyszłości wpiszemy kod zapisujący użytkownika do bazy
                // Na razie formularz tylko się waliduje
            }
        }
    }
}