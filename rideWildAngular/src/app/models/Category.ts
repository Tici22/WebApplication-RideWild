export interface Category {
   macroCategoryId: number;
   macroCategoryName: string;
   subCategories: SubCategory[];

}
interface SubCategory{
    categoryId: number;
    categoryName: string;
}