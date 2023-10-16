import { writable } from "svelte/store";

export interface IStore {
  languages: string[];
}

export const store = writable<IStore>({
  languages: []
});