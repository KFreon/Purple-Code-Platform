<script lang="ts">
  export let data: Data;


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
		const editor = monaco.editor.create(editorContainer);
		const model = monaco.editor.createModel(
			data.code, 'javascript'
		);
		editor.setModel(model);
	});

	onDestroy(() => {
		monaco?.editor.getModels().forEach((model) => model.dispose());
		editor?.dispose();
	});
</script>

<div class="card">
	<div class="title-area">
		<input type="text" placeholder="Snippet name" value="{data.snippetTitle}" readonly />
		<span>✏️</span>
		<div class="details">
			<p>
				<span>Created: {data.created}</span>
				<span>Modified: {data.modified}</span>
			</p>
			<p>
				<span>👍 {data.upvotes}</span>
				<span>👎 {data.downvotes}</span>
			</p>
		</div>
	</div>
	<div class="card-code">
		<div class="monaco-container" bind:this={editorContainer} />
		<div class="comments-container">
			<h2>Comments</h2>
			<ul>
				<li><textarea /></li>
			</ul>
		</div>
	</div>
  <button class='save-button'>Save changes</button>
</div>

<style src="./card.css"></style>
