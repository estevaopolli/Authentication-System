import { generateCode, sendCode, verifyAuth } from "./api.js";

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
    code = await GetCode();
    getCodeForm.style.display = "none";
    sendCodeForm.style.display = "block";
});

sendCodeForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    await SendCode(code);
})

async function GetCode() {
    const $email = document.querySelector("#email").value;

    const responseMessage = (await generateCode($email));
    return responseMessage.codigo;
}

async function SendCode(code) {
    const responseMessage = (await sendCode(code));
    console.log(responseMessage.message);
}