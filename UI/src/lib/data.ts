export interface Data {
  id: string;
  title: string;
  upvotes: number;
  createdOn: string;
  modifiedOn: string;
  code: string | undefined;
  comments: string[];
  languageId: string;
  email: string;
}