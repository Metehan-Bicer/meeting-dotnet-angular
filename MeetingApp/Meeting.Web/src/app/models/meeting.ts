export interface Meeting {
    id: number;
    title: string;
    description?: string;
    startDate: Date;
    endDate: Date;
    documentPath?: string;
    isCancelled: boolean;
    cancelledAt?: Date;
    createdAt: Date;
    userId: number;
}