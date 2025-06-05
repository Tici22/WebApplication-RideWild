export interface ProductDto {
    id: number; 
    productId: number;
    name: string;
    productNumber: string;
    color?: string;
    standardCost: number;
    listPrice: number;
    size?: string;
    weight?: number;
    productCategoryId?: number;
    productModelId?: number;
    sellStartDate: string;
    sellEndDate?: string;
    discontinuedDate?: string;
    rowguid: string;
    modifiedDate: string;
  }