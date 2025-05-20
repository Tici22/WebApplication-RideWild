export interface User {
    
    email: string;
    password: string;
    fullname: string;
    date: Date;
    token?: string; // Optional, if you want to include a token
}
