const API_URL = "http://192.168.1.3:5252";

export async function signup(newUser){
    const response = await fetch(`${API_URL}/signup`, {
        method: 'post',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newUser)
    });
    
    return response;
}

export async function login(user){
    const response = await fetch(`${API_URL}/login`, {
        method: 'post',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(user)
    });
    const responseMessage = await response.json();
    return responseMessage;
}

export async function logout(user) {
    localStorage.removeItem("token");
    window.location.href = "./login.html";
}

export async function generateCode(RecoverUser) {
    const response = await fetch(`${API_URL}/recover`, {
        method: 'post',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({Email: RecoverUser})
    })
    const responseMessage = await response.json();
    return responseMessage;
}

export async function sendCode(code) {
        const response = await fetch(`${API_URL}/code`, {
        method: 'post',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(code)
    })
    const responseMessage = await response.json();
    return responseMessage;
}

export async function verifyAuth(){
    const token = localStorage.getItem("token");
    const response = await fetch(`${API_URL}/profile`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    })
    return response;
}

