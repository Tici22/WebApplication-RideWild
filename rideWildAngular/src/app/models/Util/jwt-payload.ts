export interface JwtPayload {
    sub?: string; // Subject (user ID)
    userId?: string; // Name ID
    email?: string; // User email
    exp: number; // Expiration time (timestamp)
}
