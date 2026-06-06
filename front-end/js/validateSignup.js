import { signup } from "./api.js";

const $signupButton = document.querySelector("#signup-button");
$signupButton.addEventListener('click', ValidateSignUP);

async function ValidateSignUP(){
    const $username = document.querySelector("#username").value;
    const $email = document.querySelector("#email").value;
    const $password = document.querySelector("#password").value;

    const newUser = {
    username: $username,
    email: $email,
    password: $password,
    };

    const response = await signup(newUser);
    const responseMessage = await response.json();
    localStorage.setItem("token", responseMessage.token);

    if(!response.ok){
        switch (responseMessage.code){
            case "USER_ALREADY_EXISTS":
                console.log("Usuário já existe")
                document.getElementById("used-user-error").style.display = "block"
                document.getElementById("invalid-user-error").style.display = "none"
                break;

            case "EMAIL_ALREADY_EXISTS":
                console.log("Email já existe");
                document.getElementById("used-email-error").style.display = "block"
                document.getElementById("invalid-email-error").style.display = "none"
                break;

            case "INVALID_EMAIL":
                document.getElementById("invalid-email-error").style.display = "block"
                document.getElementById("used-email-error").style.display = "none"
                break;

            case "INVALID_USERNAME":
                console.log("Usuário inválido");
                document.getElementById("invalid-user-error").style.display = "block"
                document.getElementById("used-user-error").style.display = "none"
                break;

            case "INVALID_PASSWORD":
                console.log("Senha inválida");
                break;

            default:
                console.log("Erro na requisição");
                break;
        }
    }
    else{
         window.location.href = "./dashboard.html"
    }
}









