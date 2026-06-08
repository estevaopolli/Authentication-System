import { verifyAuth, logout } from "./api.js";

try {
    const authVerification = await verifyAuth();
    let $authTitle = document.querySelector("#Auth");
    let $logoutBtn = document.querySelector("#logoutBtn");
    $logoutBtn.addEventListener('click', logout);

    if (authVerification.ok){
        $authTitle.textContent = "Autenticado!"
        document.querySelector("#dashboard").hidden = false;

    }else{
        $authTitle.textContent = "Você não tem permissão para acessar essa página"
        document.location.href = "./login.html"
    }
} catch (error) {
    $authTitle.textContent = "Erro ao se conectar com o servidor!"
}