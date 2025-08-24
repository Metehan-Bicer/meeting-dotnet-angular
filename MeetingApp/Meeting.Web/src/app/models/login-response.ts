export interface LoginResponse {
    success: boolean;
    message: string;
    data: {
        userId: number;
        email: string;
        token: string;
        expiresAt: string;
    };
    errors: string[];
    statusCode: number;
}