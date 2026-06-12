import { generateCode, sendPassword, verifyAuth } from "./api.js";

try{
    const authVerification = await verifyAuth();
    if (authVerification.ok){
        window.location.href = "./dashboard.html";
    }
}
catch{
    alert("Erro ao se conectar com o servidor!");
}

const getCodeForm = document.querySelector("#get-code-form");
const sendCodeForm = document.querySelector("#send-code-form")
let code;

getCodeForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    const responseMessage = await GetCode();
    console.log(responseMessage.code);
    switch (responseMessage.code){
        case "RESET_CODE_GENERATED":
            getCodeForm.style.display = "none";
            sendCodeForm.style.display = "block";
            console.log(responseMessage.resetToken);
            break;

        case "EMAIL_NOT_FOUND":
            document.getElementById("email-not-found-error").style.display = "block";
            break;
        default:
            alert("Erro"); 
    }
});

sendCodeForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const newPassword = {
        Email: document.getElementById("email").value,
        ResetToken: document.getElementById("token").value,
        NewPassword: document.getElementById("new-password").value,
        ConfirmNewPassword: document.getElementById("confirm-new-password").value
    }

    if(document.getElementById("new-password").value == document.getElementById("confirm-new-password").value){
        await SendPassword(newPassword);

    }else{
        document.getElementById("passwords-dont-match").style.display = "block";
    }
})

async function GetCode() {
    const $email = document.querySelector("#email").value;

    const responseMessage = (await generateCode($email));
    return responseMessage;
}

async function SendPassword(newPassword) {
    const responseMessage = (await sendPassword(newPassword));
    return responseMessage;
}