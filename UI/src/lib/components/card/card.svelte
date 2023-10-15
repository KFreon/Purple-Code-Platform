<script lang="ts">
  export let data: Data;
	export let onDelete: (data: Data) => void;
	export let onSave: (data: Data) => void;

	import { onDestroy, onMount } from 'svelte';
	import type * as Monaco from 'monaco-editor/esm/vs/editor/editor.api';
	import type { Data, SearchableItem } from '$lib/data';
	import SearchableSelect from '../searchableSelect/searchableSelect.svelte';
	import type monaco from '$lib/monaco';
	let editor: Monaco.editor.IStandaloneCodeEditor;
	let monaco: typeof Monaco;
	let editorContainer: HTMLElement;
	let model: monaco.editor.ITextModel;

	let languages: SearchableItem[] = [];
	let language: SearchableItem | undefined;
	let canSave: boolean = false;

	onMount(async () => {
		// Import our 'monaco.ts' file here
		// (onMount() will only be executed in the browser, which is what we want)
		monaco = (await import('$lib/monaco')).default;

		// Your monaco instance is ready, let's display some code!
		editor = monaco.editor.create(editorContainer, {minimap: {enabled: false},  automaticLayout: true, fontLigatures: true});
		model = monaco.editor.createModel(data.code ?? '', data.languageId);
		editor.setModel(model);
		editor.onDidChangeModelContent(e => {
			data.code = model.getValue();
			checkCanSave();
		});

		languages = monaco.languages.getLanguages().map(x => ({name: x.id, value: x.id}));
		language = {name: data.languageId, value: data.languageId};

		canSave = false;
	});

	onDestroy(() => {
		editor?.getModel()?.dispose();
		editor?.dispose();
	});

	const onItemSelected = (item: SearchableItem) => {
		language = item;
		monaco.editor.setModelLanguage(model, item.name);
		checkCanSave();
	}

	const checkCanSave = () => {
		const allDataExists = !!(data.title && data.code) ?? false;
		canSave = allDataExists;
	}

	</script>

<div class="card">
	<div class="title-area">
		<input type="text" placeholder="Snippet name" bind:value={data.title} on:change={() => checkCanSave()} />
		<SearchableSelect selectedItem={language} options={languages} placeholder="Select language..." onItemSelected={onItemSelected} />
		<div></div>
		<div class="details">
			<p>
				<span>Created: {data.createdOn}</span>
				<span>Modified: {data.modifiedOn}</span>
			</p>
			<p>
				<span>👍 {data.upvotes}</span>
			</p>
			<button class='close-button' on:click={() => onDelete(data)}>❌</button>
		</div>
	</div>
	<div class="card-code">
		<div class="monaco-container" bind:this={editorContainer} />
		<div class="comments-container">
			<h2>Comments</h2>
			<p>TODO</p>
		</div>
	</div>
  <button class='save-button' disabled={!canSave} on:click={() => onSave(data)}>Save changes</button>
</div>

<style src="./card.css"></style>
