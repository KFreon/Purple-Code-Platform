<script lang="ts">
  export let data: Data;
	export let onDelete: (data: Data) => void;

	import { onDestroy, onMount } from 'svelte';
	import type * as Monaco from 'monaco-editor/esm/vs/editor/editor.api';
	import type { Data } from '$lib/data';
	let editor: Monaco.editor.IStandaloneCodeEditor;
	let monaco: typeof Monaco;
	let editorContainer: HTMLElement;

	onMount(async () => {
		// Import our 'monaco.ts' file here
		// (onMount() will only be executed in the browser, which is what we want)
		monaco = (await import('../lib/monaco')).default;

		// Your monaco instance is ready, let's display some code!
		editor = monaco.editor.create(editorContainer, {automaticLayout: true});
		const model = monaco.editor.createModel(data.code, 'javascript');
		editor.setModel(model);
	});

	onDestroy(() => {
		editor?.getModel()?.dispose();
		editor?.dispose();
	});
</script>

<div class="card">
	<div class="title-area">
		<input type="text" placeholder="Snippet name" value="{data.snippetTitle}" />
		<div class="details">
			<p>
				<span>Created: {data.created}</span>
				<span>Modified: {data.modified}</span>
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
			<ul>
				<li><textarea /></li>
			</ul>
			<button>Save comment? Have blank input?</button>
		</div>
	</div>
  <button class='save-button'>Save changes</button>
</div>

<style src="./card.css"></style>
