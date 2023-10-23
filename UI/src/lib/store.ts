import { writable } from "svelte/store";

export interface IStore {
  languages: string[];
  theme: 'light' | 'dark';
}

export const store = writable<IStore>({
  languages: [],
  theme: 'dark'
});