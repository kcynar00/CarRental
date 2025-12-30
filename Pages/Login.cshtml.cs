using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        // Wstrzykujemy SignInManager, żeby móc sprawdzać hasła w bazie
        public LoginModel(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        // Klasa inputów (tylko to, co potrzebne do logowania)
        public class InputModel
        {
            [Required(ErrorMessage = "Email jest wymagany")]
            [EmailAddress(ErrorMessage = "Niepoprawny format email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Hasło jest wymagane")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            
            // Jeśli użytkownik jest już zalogowany, wyczyść ciasteczko (opcjonalne, dla bezpieczeństwa)
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Próba logowania
                // false, false = nie zapamiętuj (persistent cookie), nie blokuj konta po błędach
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Sukces - przekieruj np. na stronę główną
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    // Błąd - wyświetl komunikat
                    ModelState.AddModelError(string.Empty, "Nieudana próba logowania. Sprawdź email i hasło.");
                    return Page();
                }
            }

            // Jeśli coś poszło nie tak (walidacja), wyświetl formularz ponownie
            return Page();
        }
    }
}