import { verifyAuth } from "./api.js";
let $authTitle = document.querySelector("#Auth");

try {
    const authVerification = await verifyAuth();
    if (authVerification.ok){
        console.log("Autenticado")
        $authTitle.textContent = "Autenticado!"
    }else{
        $authTitle.textContent = "Você não tem permissão para ver essa página"
    }
} catch (error) {
    $authTitle.textContent = "Erro ao se conectar com o servidor!"
}