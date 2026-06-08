import { login, verifyAuth } from "./api.js";

try{
    const authVerification = await verifyAuth();
    if (authVerification.ok){
        window.location.href = "./dashboard.html";
    }
}
catch{
    alert("Erro ao se conectar com o servidor!");
}

const $loginButton = document.querySelector("#login-button");
$loginButton.addEventListener('click', ValidateLogin);

async function ValidateLogin() {
    const $email = document.querySelector("#email").value;
    const $password = document.querySelector("#password").value;

    const user = {
        Email: $email,
        Password: $password
    };

    const responseMessage = await login(user);
    switch (responseMessage.code) {
        case "INCORRECT_CREDENTIALS":
            document.getElementById("incorrect-credentials-error").style.display = "block";
            break;
        case "SUCCESSFUL_VALIDATION":
            localStorage.setItem("token", responseMessage.token);
            window.location.href = "./dashboard.html";
    }
    console.log(responseMessage.code);
}