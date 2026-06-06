const API_URL = "https://localhost:7252";

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

export async function verifyAuth(){
    const token = localStorage.getItem("token");
    const response = await fetch(`${API_URL}/profile`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    })
    return response;
}
