<script lang="ts">
	import type { Data } from '$lib/data';
	import { onMount } from 'svelte';
	import Card from './card.svelte';

	let cards: Data[] = [];

  let promise: Promise<unknown>;

  onMount(() => {
    // Get data
    promise = fetch('https://localhost:5001/snippets')
      .then(resp => resp.json())
      .then(x => {
        console.log('data', x);
        cards = x;
        return cards;
      });
  });

	const emptyCardData: Data = {
		id: new Date().toISOString(),
		upvotes: 0,
		code: '',
		comments: [],
		createdOn: new Date().toLocaleDateString('en-au', {year: 'numeric', month: '2-digit', day: '2-digit'}),
		modifiedOn: new Date().toLocaleDateString('en-au', {year: 'numeric', month: '2-digit', day: '2-digit'}),
		title: ''
	};

	function addNewSnippet() {
		cards = [emptyCardData, ...cards];
	}

	function deleteSnippet(data: Data) {
		cards = cards.filter((x) => x.title !== data.title);
	}

  function saveSnippet(snippet: Data) {
    console.log(snippet)
    snippet.id = snippet.title;
    promise = fetch('https://localhost:5001/snippets', {
      method: 'POST',
      body: JSON.stringify(snippet),
      headers: {
        "content-type": 'application/json'
      }
    })
  }
</script>

<div class="main-container">
	<div class="header">
		<h1>Purple Code Platform</h1>
		<p>We write so much code, but sometimes we lose track of the good bits.</p>
		<p>Here we can save the bits that others (or just us) might want to use again later.</p>
	</div>

	{#await promise}
  <div class='loading'>
    <p>🍵</p>
  </div>
  {:then resp} 
  <button class="new-button" on:click={addNewSnippet}>Add new</button>
	<ul class="code-area">
		{#each cards as card (card.id)} <!-- keyed so we can prepend -->
			<Card data={card} onDelete={(item) => deleteSnippet(item)} onSave={(item) => saveSnippet(item)} />
		{/each}
	</ul>
  {/await}
</div>

<style src="./page.css"></style>
