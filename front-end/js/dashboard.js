import { verifyAuth } from "./api.js";

const authVerification = await verifyAuth();
if (authVerification.ok){
    console.log("Autorizado");
}
else{
    console.log("Não autorizado")
}